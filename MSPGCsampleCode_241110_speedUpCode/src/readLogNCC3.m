function [LogData] = readLogNCC3(path_log, key_log)

    %UNTITLED5 이 함수의 요약 설명 위치
    %   자세한 설명 위치
    
    % Last Updated: 2022-04-02 23:45
    % 시작시간(sec) / 끝시간(sec) / idx layer / idx tuning / idx part (파트 없을 경우 0) / idx resume / x pos / y pos 
    
    %% List-up un-sorted log files(Record & Specif)
     
    ds = tabularTextDatastore(path_log,'FileExtensions','.csv');                
    chk_consideredData= contains(ds.Files, key_log);                            
    chk_record = contains(ds.Files, "record");                                 
    recordData = chk_consideredData&chk_record;                                 
    Log_Record_Unsorted = ds.Files(recordData);                                
    
    chk_specif = contains(ds.Files, "specif");                               
    chk_considered_specif = chk_consideredData&chk_specif;                    
    Log_Specif_Unsorted = ds.Files(chk_considered_specif);                     
    
    %% Parameter extract in (Log_Config)
    Parameters = struct;
    
    chk_config= contains(ds.Files, "config");                                   
    chk_considered_config = chk_consideredData&chk_config;                      
    configData = ds.Files(chk_considered_config);                               
    
    lines = readlines(configData{1});

    refLine = find(strcmp(lines, '# SAD - M-id 21900'));
    
    sad_x = extract(lines(refLine+1,:),";" + digitsPattern(4) + "." + digitsPattern(1));
    sad_x = str2double(extractAfter(sad_x, 1));
    sad_y = extract(lines(refLine+1,:), "," + digitsPattern(4) + "." + digitsPattern(1));
    sad_y = str2double(extractAfter(sad_y, 1));
    
    distICtoIso_x = extract(lines(refLine+2,:),";" + digitsPattern(4) + "." + digitsPattern(2));
    distICtoIso_x =str2double( extractAfter(distICtoIso_x, 1));
    distICtoIso_y = extract(lines(refLine+2,:), "," + digitsPattern(4) + "." + digitsPattern(2));
    distICtoIso_y = str2double(extractAfter(distICtoIso_y, 1));
    
    coeff_x = sad_x/(sad_x - distICtoIso_x);
    coeff_y = sad_y/(sad_y - distICtoIso_y);
    
    Parameters.config.coeff_x = coeff_x;
    Parameters.config.coeff_y = coeff_y;
    
    %% Setting
    
    % Return Data
    LogData = [];
    
    % Figure Setting
%     figure; hold on; grid on;
%     title('Irradiation time sturcture (log)', 'fontsize',12, 'fontweight', 'bold')
%     xlabel('Time [sec]', 'fontsize', 12, 'fontweight', 'bold')
%     ylabel('Beam on/off', 'fontsize', 12, 'fontweight', 'bold')
    
    % Get Reference Time (First Tuning Beam On Time)
    FirstTuningLogName = GetFirstTuningLogName(Log_Record_Unsorted);
    Parameters.timeREF = GetRefTime(FirstTuningLogName);
    
    %% Extract Log File Information
    
    % For문: Layer
    %  i) part 파일이 존재하는 경우
    %    For문: part number
    %       Tuning -> Normal -> Resume 순으로 로그 파일 정렬
    %       정렬한 로그파일을 한 개씩 읽어나감
    %  ii) part 파일이 없는 경우
    %       Tuning -> Normal -> Resume 순으로 로그 파일 정렬
    %       정렬한 로그파일을 한 개씩 읽어나감
    
    % 1. BeamOnTime / 2. BeamOffTime / 3. idx_layer / 4. idx_tuning / 5. idx_part / 6. idx_resume / 7. Xpos / 8. Ypos
    
    Log_LayerClassified = SortLogbyLayer(Log_Record_Unsorted);
    NumOfLayer = size(Log_LayerClassified, 2);
    
    for i = 1 : NumOfLayer
    
%         fprintf('[Layer %d start] \n', i);
        Log_Layer = Log_LayerClassified{1, i};    
    
        % Check Existence of 'part' Log File
        if any(contains(Log_Layer, "part"))
        
            [Log_ClassifiedbyPartNumber, NumOfPart] = ClassifyLogbyPartNumber(Log_Layer);
                
            for j = 1 : NumOfPart
    
                Log_Part = Log_ClassifiedbyPartNumber{j, 1};
                [Log_temp_sorted, NumOfLog] = SortLogbyTuningNormalResume(Log_Part);
    
                for k = 1 : NumOfLog
    
                    % Get Log File Name(record, speicf)
                    Log_record = Log_temp_sorted{k};
                    Log_Specif = strrep(Log_record, "_record_", "_specif_");    
    
                    % Get Log_specif data(SMX_OFFSET, SMY_OFFSET, ICX_OFSSET, ICY_OFFSET)
                    Parameters.specif = ReadLogFile_specif(Log_Specif);
                    
                    % Get Log_record data
                    [BeamTime, idx, BeamPosi] = ReadLogFile_record(Log_record, Parameters);    
                    LogData_Single = [BeamTime, idx, BeamPosi];
    
                    % Cumulate Log_record data
                    LogData = [LogData; LogData_Single];
    
                    % Print out Program status    
                    LogInfoPos = strfind(Log_record, "record_") + 6;
                    LogName = extractAfter(Log_record, LogInfoPos);
%                     fprintf('  %s Finished \n', LogName);
    
                end % Single log end
            end % Single Layer(Part o) Tuning&Normal&Resume end
    
        else
    
            % Sort Log File by Tuning->Normal->Resume order
            [Log_temp_sorted, NumOfLog] = SortLogbyTuningNormalResume(Log_Layer);
    
            for k = 1 : NumOfLog                
    
                % Get Log File Name(record, speicf)
                Log_record = Log_temp_sorted{k};
                Log_Specif = strrep(Log_record, "_record_", "_specif_");    

                % Get Log_specif data(SMX_OFFSET, SMY_OFFSET, ICX_OFSSET, ICY_OFFSET)
                Parameters.specif = ReadLogFile_specif(Log_Specif);
                
                % Get Log_record data
                [BeamTime, idx, BeamPosi] = ReadLogFile_record(Log_record, Parameters);    
                LogData_Single = [BeamTime, idx, BeamPosi];

                % Cumulate Log_record data
                LogData = [LogData; LogData_Single];

                % Print out Program status    
                LogInfoPos = strfind(Log_record, "record_") + 6;
                LogName = extractAfter(Log_record, LogInfoPos);
%                 fprintf('  %s loaded \n', LogName);
    
            end % Single log end
        end % Single Layer(Part x) Tuning&Normal&Resume end

    end % Total log end
    
    ylim([0 1.2]);
%     fprintf('******* log file loading finished ******* \n');

end

%% Additioanal Function
function t = etime_jaerin(t1,t0)

    t = (t1(:,1:4) - t0(:,1:4))*[3600; 60; 1; 0.001];

end

function LogName = GetFirstTuningLogName(Log)

TuningIndex = find(contains(Log, "tuning") == 1);
FirstTuningIndex = TuningIndex(1);
LogName = Log(FirstTuningIndex);

end

function timeREF = GetRefTime(FirstTuningFileName) % hh mm ss xxx
    
    % Load Excel(Converted Log)
    rawData = readmatrix(cell2mat(FirstTuningFileName), 'Delimiter', ',', 'OutputType', 'char');
    rawData = rawData(1 : (length(rawData) - 10), :);
    
    % Extract Beam Irradiation Timing (using pos information)
    pos = str2double(rawData(:, [5. 6]));
    chk_BeamIrradiation = pos(:, 1) ~= -10000 | pos(:, 2) ~= -10000;
    Idx_BeamIrradiation = find(chk_BeamIrradiation == 1);
    
    % Get First Tuning Beam Irradiation Start Timing
    timeREF_Index = rawData(Idx_BeamIrradiation(1), 2);
    timeREF_temp1 = strsplit(cell2mat(timeREF_Index), " ");
    timeREF_temp2 = timeREF_temp1{1, 2};                            % hh:mm:ss:xxx
    timeREF = str2num(strrep(timeREF_temp2, ':', ' '));             % hh mm ss xxx

end

function [Log_PartClassified, NumOfPart] = ClassifyLogbyPartNumber(Log_Layer)

    partNumberPos = cell2mat(strfind(Log_Layer, "part_")) + 5;
    partNumber = str2double(extractBetween(Log_Layer, partNumberPos, partNumberPos+1));    
    NumOfPart = size(unique(partNumber), 1);

    Log_PartClassified = cell(NumOfPart, 1);
    for idx = 1 : NumOfPart
    
        index_part = find(partNumber == idx);
        Log_PartClassified{idx, 1} = Log_Layer(index_part);
    
    end
end

function [Log_temp_sorted, numOfLog] = SortLogbyTuningNormalResume(Log_PartClassified)

    Log_temp = Log_PartClassified;

    % sort by "Tuning -> Noraml -> Resume" order
    isTuning = contains(Log_temp, "tuning");
    isResume = contains(Log_temp, "resume");
    isNormal = (~isTuning & ~isResume);

    Log_temp_sorted = [Log_temp(isTuning); Log_temp(isNormal); Log_temp(isResume)];
    numOfLog = size(Log_temp_sorted, 1);
end

function [Time_raw, Xpos_raw, Ypos_raw, DataSize] = ReadLogFile(Log)

    rawData = readmatrix(Log, 'Delimiter', ',', 'OutputType', 'char');
    rawData = rawData(1 : end-8, :); % delete bottom row

    Time_raw = rawData(:, 2);
    Xpos_raw = str2double(rawData(:, 5));
    Ypos_raw = str2double(rawData(:, 6));
    DataSize = size(Time_raw, 1);
end

function Time_ConvertedFormat = ConvertTime_raw(Time_raw, timeREF)

    Time_ConvertedFormat = zeros(size(Time_raw, 1), 1);
    for l = 1 : size(Time_raw, 1)
        Time_raw_split = strsplit(Time_raw{l, 1}, " ");
        Time_hh_mm_ss_xxx = str2num(strrep(Time_raw_split{1, 2}, ':', ' '));
        Time_reformat = etime_jaerin(Time_hh_mm_ss_xxx, timeREF);
        Time_ConvertedFormat(l, 1) = Time_reformat;
    end
end

function BeamType = GetBeamType(Log)

    if contains(Log, "tuning")
        BeamType = 'Tuning';
    elseif contains(Log, "resume")
        BeamType = 'Resume';
    else
        BeamType = 'Normal';
    end

end

function plotBeamTimeStructure(Time_ConvertedFormat, chk_BeamOn, BeamType)

    switch BeamType
        case 'Tuning'
            Color = 'b';
        case 'Normal'
            Color = 'r';
        case 'Resume'
            Color = 'm';
    end

%     plot(Time_ConvertedFormat, chk_BeamOn, Color, 'displayname', BeamType);
end

function param = ReadLogFile_specif(Log_Specif)
    
    specifVars = readmatrix(Log_Specif, 'Range', 'A4:D4');

    param = struct;

    param.SMX_OFFSET = specifVars(1,1);
    param.SMY_OFFSET = specifVars(1,2);
    param.ICX_OFFSET = specifVars(1,3);
    param.ICY_OFFSET = specifVars(1,4);
end

function [BeamTime, idx, BeamPosi] = ReadLogFile_record(Log_record, param)

    BeamTime = [];
    BeamPosi = [];

    BeamType = GetBeamType(Log_record);
    

    % Log Data Read
    [Time_raw, Xpos_raw, Ypos_raw, ~] = ReadLogFile(Log_record);
    Time_ConvertedFormat = ConvertTime_raw(Time_raw, param.timeREF);

    % Get Beam Time
    chk_BeamOn = Xpos_raw ~= -10000 | Ypos_raw ~= -10000;
    BeamOnIndex = find((diff(chk_BeamOn) == 1) == 1);
    BeamOffIndex = find((diff(chk_BeamOn) == -1) == 1);

    NumOfBeam = size(BeamOnIndex, 1);
    for m = 1 : NumOfBeam

        % Get Beam Time
        BeamTime_temp = [Time_ConvertedFormat(BeamOnIndex(m)) Time_ConvertedFormat(BeamOffIndex(m))];

        % Get Beam Position
        Xpos_chk = Xpos_raw(BeamOnIndex(m) : BeamOffIndex(m));
        Xpos_valid = Xpos_chk(Xpos_chk ~= -10000);
        Xpos = mean(Xpos_valid);
        if isnan(Xpos)
            Xpos = 0;
            warning(['error in recording spot x position in log file ' Log_record])
        end

        Ypos_chk = Ypos_raw(BeamOnIndex(m) : BeamOffIndex(m));
        Ypos_valid = Ypos_chk(Ypos_chk ~= -10000);
        Ypos = mean(Ypos_valid);
        if isnan(Ypos)
            Ypos = 0;
            warning(['error in recording spot y position in log file ' Log_record]);
        end

        % Beam Position Correction
        Xpos_cor = (Xpos - param.specif.ICX_OFFSET) * param.config.coeff_x;
        Ypos_cor = (Ypos - param.specif.ICY_OFFSET) * param.config.coeff_y;

        BeamPosi_temp = [Ypos_cor Xpos_cor];

        % Add data
        BeamTime = [BeamTime; BeamTime_temp];
        BeamPosi = [BeamPosi; BeamPosi_temp];

    end % Single log extract finished

    idx = repmat(GetLogIdxInfo(Log_record), NumOfBeam, 1);

%     plotBeamTimeStructure(Time_ConvertedFormat, chk_BeamOn, BeamType);

end

function idx = GetLogIdxInfo(Log)

    LayerNumberPos = strfind(Log, "record_") + 7;
    idx_Layer = str2double(extractBetween(Log, LayerNumberPos, LayerNumberPos+3)) + 1;

    if contains(Log, 'tuning')
        TuningNumberPos = strfind(Log, "tuning_") + 7;
        idx_Tuning = str2double(extractBetween(Log, TuningNumberPos, TuningNumberPos+2));
    else
        idx_Tuning = 0;
    end

    if contains(Log, 'part')
        PartNumberPos = strfind(Log, "part_") + 5;
        idx_Part = str2double(extractBetween(Log, PartNumberPos, PartNumberPos+1));
    else
        idx_Part = 0;
    end

    if contains(Log, 'resume')
        ResumeNumberPos = strfind(Log, "resume_") + 7;
        idx_Resume = str2double(extractBetween(Log, ResumeNumberPos, ResumeNumberPos+2));
    else
        idx_Resume = 0;
    end

    idx = [idx_Layer, idx_Tuning, idx_Part, idx_Resume];
end

function Log_LayerClassified = SortLogbyLayer(Log_Record_Unsorted)

    NumOfLogFile = length(Log_Record_Unsorted);
    Relation_LogToLayer = zeros(NumOfLogFile, 1);
    
    for idx = 1 : NumOfLogFile
    
        SingleLog_Name = strsplit(cell2mat(Log_Record_Unsorted(idx)), "record_");
        SingleLog_Layer = str2double(extractBefore(SingleLog_Name{1, 2}, 5)) + 1;
        Relation_LogToLayer(idx, 1) = SingleLog_Layer;
    
    end
    
    NumOfLayer = size(unique(Relation_LogToLayer), 1);
    
    Log_LayerClassified = cell(1, NumOfLayer);
    for idx = 1 : NumOfLayer
    
        index_layer = find(Relation_LogToLayer == idx);
        Log_LayerClassified{1, idx} = Log_Record_Unsorted(index_layer);
    
    end
end
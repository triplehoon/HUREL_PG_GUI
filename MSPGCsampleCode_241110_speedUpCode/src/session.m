classdef session < matlab.mixin.Copyable
    % this class is for energy calibration of multi-slit camera
    %   자세한 설명 위치

    properties       
        gridFoV
        gridFoVmm
        numDets
        beamDir % 1: right, 0:left
        gridDepth
        cameraOffset
        cameraDepth

        detectors_calib
        eWindow

        listPG_ungated
        listPG
        listPG1
        listPG2
        spotPG_unsorted
        spotPG
        spotPGmm

        trigSignal
        trigSignal1
        trigSignal2
        dataLog
        dataPld
        dataSet
        dataDAQ

        timeTrig1
        timeTrig2
        timeTrig
        timeTrig_matched

        layerEnergies
        numLayer

        dataSet_agg
        spotPG_agg

    end

    methods


        function obj = session()
            obj.gridFoV = -106.8266:3.0092:106.8266;
            obj.gridFoVmm = (obj.gridFoV(1:end-1) + obj.gridFoV(2:end))/2; %movemean 좌표
            obj.numDets = 0:143;
        end

        function obj = setEnergyWindow(obj, detectors_calib)

            obj.detectors_calib = detectors_calib;

            obj.eWindow=zeros(144,2);
            lld = 3000;
            uld = 10000;
            for idxDet = obj.numDets
                a = obj.detectors_calib(idxDet+1).fitobject.p1;
                b = obj.detectors_calib(idxDet+1).fitobject.p2;
                obj.eWindow(idxDet+1,1) = a*lld+b;
                obj.eWindow(idxDet+1,2) = a*uld+b;
            end

        end

        function obj = loadDataSet(obj, pgFileName, logFilePath, pldFileName, pld3DFileName) % loading 분리
            obj.loadPGfile(pgFileName);
            obj.loadLogFolder(logFilePath);
            obj.loadPldFiles(pldFileName, pld3DFileName);            
        end

        function obj = postProcessDataSet(obj, cameraOffset, cameraDepth, beamDir)
            obj.processDaqData;
            obj.beamDir = beamDir;
            obj.mergeLogPld; % 스팟 별 기록 1) start, 2) end, 3) layer, 4) tune, 5) part, 6) resume, 7) x (log), 8) y (log), 9) layer (plan), 10) E, 11) x (plan), 12) y (plan), 13) z (plan), 14) mu, 15) range diff, 16) agg. mu 17) trueRange80
            obj.genTrig;
            obj.energyGating(obj.eWindow);
            % obj.splitPGintoSpots_v2;
            obj.splitPGintoSpots_v5_optimized;
            obj.sortPGdist;
            obj.setGridDepth(cameraOffset, cameraDepth);
        end

        %{
        function obj = loadData(obj, dataList, pathData, numCase, eWindow, beamDir, cameraOffset, cameraDepth) %구버전
            dataLine = find(dataList.numCase == numCase);
            obj.loadDAQFile([pathData '\pg'], [dataList.pgfilename{dataLine} '.bin'], beamDir);
            obj.loadLogFile([pathData '\log'], dataList.logFileName{dataLine});
            obj.loadPldFile([pathData '\pld'], dataList.pldFileName{dataLine}, [pathData '\pld3d'], dataList.pld3DFileName{dataLine});
            obj.mergeLogPld; % 스팟 별 기록 1) start, 2) end, 3) layer, 4) tune, 5) part, 6) resume, 7) x (log), 8) y (log), 9) layer (plan), 10) E, 11) x (plan), 12) y (plan), 13) z (plan), 14) mu, 15) range diff, 16) agg. mu 17) trueRange80
            obj.genTrig;
            obj.energyGating(eWindow);
            obj.splitPGintoSpots;
            obj.sortPGdist;
            obj.setGridDepth(cameraOffset, cameraDepth);
        end
        %}


        %         function obj = loadData_noTrigSignal(obj, dataList, pathData, numCase, eWindow, beamDir, cameraDepth)
        %             dataLine = find(dataList.numCase == numCase);
        %             obj.loadDAQFile([pathData '\pg'], [dataList.pgfilename{dataLine} '.bin'], beamDir);
        %             obj.loadLogFile([pathData '\log'], dataList.logFileName{dataLine});
        %             obj.loadPldFile([pathData '\pld'], dataList.pldFileName{dataLine}, [pathData '\pld3d'], dataList.pld3DFileName{dataLine});
        %             obj.mergeLogPld; % 스팟 별 기록 1) start, 2) end, 3) layer, 4) tune, 5) part, 6) resume, 7) x (log), 8) y (log), 9) layer (plan), 10) E, 11) x (plan), 12) y (plan), 13) z (plan), 14) mu, 15) range diff, 16) agg. mu 17) trueRange80
        %             obj.genTrigByCountRate;
        %             %             load(eWindow_fineTuned.mat, eWindow);
        %             obj.energyGating(eWindow);
        %             obj.splitPGintoSpots;
        %             obj.sortPGdist;
        %             obj.setGridDepth(cameraDepth);
        %         end

        function [] = loadPGfile(obj, filename_pg) % 신버전
            
            [~,name,~] = fileparts(filename_pg);
            tic
            fprintf(['loading "' name '" ... '])
            [obj.dataDAQ] = loadPgBinFile(filename_pg);            
            fprintf('finished!')
            toc
        end
        function [] = processDaqData(obj)
            fprintf(['Post process daq data' ' ... '])
            tic;
            % 여기도 개선 필요
            obj.listPG_ungated = obj.dataDAQ(min(obj.numDets)<=obj.dataDAQ(:,1) & obj.dataDAQ(:,1)<=max(obj.numDets),:);
            obj.listPG_ungated = obj.listPG_ungated (obj.listPG_ungated(:,2) > 0,:); %시간 0 이상
            obj.listPG_ungated(:,2) = obj.listPG_ungated(:,2)*1E-9; % ns -> sec
            obj.listPG_ungated = obj.listPG_ungated (obj.listPG_ungated(:,2) < 86400,:); %시간 24 시간 미만
            obj.listPG_ungated = obj.listPG_ungated(obj.listPG_ungated(:,3) < 16384,:);

            %             figure; hold on; box on; grid on;
            %             scatter(obj.listPG_ungated(:,2),1:length(obj.listPG_ungated), '.');
            %             ylabel('cumulated number of PG count')
            %             xlabel('time (second)');
            %             hold off

            obj.listPG_ungated = sortrows(obj.listPG_ungated, 2);
            obj.trigSignal1 = obj.dataDAQ(obj.dataDAQ(:,1)==144,2:3);
            obj.trigSignal1(:,1) = obj.trigSignal1(:,1)*1E-9;% ns -> sec
            fprintf('finished!')
            toc
        end
        %{
        function [] = loadDAQFile(obj, path_pg, filename_pg, beamDir) % 구버전
            tic
            fprintf(['loading "' filename_pg '" ... '])
            [dataDAQ] = loadPgBinFile(path_pg, filename_pg);
            obj.listPG_ungated = dataDAQ(min(obj.numDets)<=dataDAQ(:,1) & dataDAQ(:,1)<=max(obj.numDets),:);
            obj.listPG_ungated = obj.listPG_ungated (obj.listPG_ungated(:,2) > 0,:); %시간 0 이상
            obj.listPG_ungated(:,2) = obj.listPG_ungated(:,2)*1E-9; % ns -> sec
            obj.listPG_ungated = obj.listPG_ungated (obj.listPG_ungated(:,2) < 86400,:); %시간 24 시간 미만
            obj.listPG_ungated = obj.listPG_ungated(obj.listPG_ungated(:,3) < 16384,:);

            %             figure; hold on; box on; grid on;
            %             scatter(obj.listPG_ungated(:,2),1:length(obj.listPG_ungated), '.');
            %             ylabel('cumulated number of PG count')
            %             xlabel('time (second)');
            %             hold off

            obj.listPG_ungated = sortrows(obj.listPG_ungated, 2);
            obj.trigSignal1 = dataDAQ(dataDAQ(:,1)==144,2:3);
            obj.trigSignal1(:,1) = obj.trigSignal1(:,1)*1E-9;% ns -> sec
            obj.beamDir = beamDir;
            fprintf('finished!')
            toc
        end
        %}

        function [] = loadLogFolder(obj, path_log)
            tic
            newStr = split(path_log, '\');
            fprintf(['loading log files from "' newStr{end-1} '\' newStr{end} '" ... '])
            obj.dataLog = readLogNCC3(path_log, "");
            fprintf('finished!');
            toc
        end

        %{
        function [] = loadLogFile(obj, path_log, key_log)
            tic
            fprintf(['loading log files"' key_log '" ... '])
            obj.dataLog = readLogNCC3(path_log, key_log);
            fprintf('finished!');
            toc
        end
        %}

        function [] = loadPldFiles(obj, file_pld, file_pld3d)
            tic
            fprintf(['loading pld' '...'])
            [~,file_pld_name,~] = fileparts(file_pld);
            [~,file_3dpld_name,~] = fileparts(file_pld3d);
            isPldExist =  ~isempty(file_pld);
            isPld3dExist = ~isempty(file_pld3d);
            if isPldExist && isPld3dExist % both exists
                data_plan = readSpotData(file_pld, file_pld3d);
                %fprintf([' pld file ', file_pld_name, ' and 3D spot file ', file_3dpld_name,  ' loaded!\n'] );
            elseif isPldExist % only 2d pld exists
                data_plan = readSpotData_onlyPLD(file_pld);
                %fprintf([' pld file ', file_pld_name ' loaded!\n'] );
            else
                data_plan = nan;
                %fprintf('No pld file loaded!\n');
            end
            obj.dataPld = data_plan(data_plan(:,6)~=0,:);
            obj.layerEnergies = sortrows(unique(obj.dataPld(:,2)), 'descend');
            obj.numLayer = length(obj.layerEnergies);
            fprintf('finished!');
            toc
        end
        %{
        function [] = loadPldFile(obj, path_pld, file_pld, path_pld3d, file_pld3d)
            tic
            fprintf(['loading pld file ... '])
            isPldExist =  ~isempty(file_pld);
            isPld3dExist = ~isempty(file_pld3d);
            if isPldExist && isPld3dExist % both exists
                data_plan = readSpotData([path_pld, '\', file_pld,'.pld'], [path_pld3d, '\', file_pld3d,'.pld']);
                fprintf([' pld file ', file_pld, ' and 3D spot file ', file_pld3d,  ' loaded!\n'] );
            elseif isPldExist % only 2d pld exists
                data_plan = readSpotData_onlyPLD([path_pld, '\', file_pld,'.pld']);
                fprintf([' pld file ', file_pld ' loaded!\n'] );
            else
                data_plan = nan;
                fprintf('No pld file loaded!\n');
            end
            obj.dataPld = data_plan(data_plan(:,6)~=0,:);
            obj.layerEnergies = sortrows(unique(obj.dataPld(:,2)), 'descend');
            obj.numLayer = length(obj.layerEnergies);
            toc
        end
        %}

        function [] = mergeLogPld(obj)
            tic
            fprintf('merging log and pld files ...')
            dataset = obj.dataLog;
            numLayer = max(obj.dataLog(:,3));
            for idx_layer = 1:numLayer
                count = 0;
                for idx_logline = find(obj.dataLog(:,3)==idx_layer)'
                    chk = 0;
                    for idx_pld=find(obj.dataPld(:,1)==idx_layer)'
                        plan_x = obj.dataPld(idx_pld,3);
                        plan_y = obj.dataPld(idx_pld,4);
                        log_x = obj.dataLog(idx_logline,7);
                        log_y = obj.dataLog(idx_logline,8);
                        if ( sqrt((log_x-plan_x)^2 + (log_y-plan_y)^2) < 5)
                            dataset(idx_logline,9:14) = obj.dataPld(idx_pld,:);
                            chk = 1; %거리가 너무 먼 스팟이 있는 경우 경고 발생시킨다.
                            break;
                        elseif (obj.dataLog(idx_logline,4) ~= 0) %튜닝빔인 경우는 warning 발생시키지 않는다.
                            dataset(idx_logline,9:10) = obj.dataPld(idx_pld,1:2);
                            chk = 1;
                        end
                    end
                    if ~chk
                        count = count+1;
                    end
                end
                if count
                    warning('spot positions in plan and log are too far from each other on layer %d , (%d spots)', idx_layer,count);
                end
                %     figure;hold on;box on;
                %     data_plan_templayer = data_plan(find(data_plan(:,1)==idx_layer),:);
                %     data_log_templayer = data_log(find(data_log(:,3)==idx_layer),:);
                %     scatter(data_plan_templayer(:,3),data_plan_templayer(:,4),'*','DisplayName','Plan');
                %     scatter(data_log_templayer(:,7),data_log_templayer(:,8),'o','DisplayName','Log');
                %     legend;
                %     title(['layer ' num2str(idx_layer)]);
                %     hold off;
            end
            obj.dataSet = dataset;
            fprintf('finished!')
            toc
        end

        function [] = genTrig(obj) % 만약 144 데이터 없으면 countrate로 변경. threshold 낮게 잡고 트리거 많이 생성되게 한 후, 가까운 것 가져오게 하기.
            if isempty(obj.trigSignal1)
                warning('trigger singal was not recorded')
                obj.genTrigByCountRate;
                return
            end
            tic
            fprintf('generating trigger using trigger signal ...')
            th_high = 800;
            th_low = 600;
            timeTrigger = zeros(1,2);
            state_trig = 0;
            for idx = 1:length(obj.trigSignal1)
                switch state_trig
                    case 0
                        if obj.trigSignal1(idx,2) > th_high
                            state_trig = 1;
                            timeTrigger(end+1,1) = obj.trigSignal1(idx-20);
                        end
                    case 1
                        if obj.trigSignal1(idx,2) < th_low
                            state_trig = 0;
                            timeTrigger(end,2) = obj.trigSignal1(idx);
                        end
                end
            end
            obj.timeTrig1 = timeTrigger(2:end,:);
            if length(obj.timeTrig1) > length(obj.dataLog)
                % warning(['trigger spots (' num2str(length(obj.timeTrig1)) ') > log spots (' num2str(length(obj.dataLog)) ')'])
                % disp('extra trigger spots were eliminated');
            elseif length(obj.timeTrig1) < length(obj.dataLog)
                fprintf('failed !\n')
                warning(['trigger spots (' num2str(length(obj.timeTrig1)) ') < log spots (' num2str(length(obj.dataLog)) ')'])
                fprintf('switching trigger generating method (using PG count rate)\n')
                obj.genTrigByCountRate;
                return
            end
            fprintf('finished!')
            toc
        end

        function [] = genTrigByCountRate(obj)
            tic
            fprintf('generating trigger using PG count rate ...')
            binWidth = 0.0001; %0.1ms
            th_high = 35;
            th_low = 5;
            [sumPGcount, timeBins] = histcounts(obj.listPG_ungated(:,2), 0:binWidth:max(obj.listPG_ungated(:,2)));
            sumPGcount_mm = smoothdata(sumPGcount,3);
            timeTrigger = zeros(1,2);
            state_trig = 0;
            for idx = 1:length(sumPGcount_mm)
                switch state_trig
                    case 0
                        if sumPGcount_mm(idx) > th_high
                            state_trig = 1;
                            timeTrigger(end+1,1) = max(timeTrigger(end,2)+0.0001, timeBins(idx-10));
                        end
                    case 1
                        if sumPGcount_mm(idx) < th_low
                            state_trig = 0;
                            timeTrigger(end,2) = timeBins(idx+1);
                        end
                end
            end
            obj.timeTrig1 = timeTrigger(2:end,:);
            fprintf('finished!')
            toc
        end
        function [] = energyGating(obj, eWindow)
            % old. 3 seconds
            % tic
            % dataPG_gated = zeros(1,3);
            % for idx_det = obj.numDets
            %     detPG_a = obj.listPG_ungated(obj.listPG_ungated(:,1)==idx_det,:);
            %     chkE = detPG_a(:,3) > eWindow(idx_det+1,1) & detPG_a(:,3) < eWindow(idx_det+1,2);
            %     dataPG_gated = vertcat(dataPG_gated, detPG_a(chkE,:));
            % end
            % obj.listPG1 = sortrows(dataPG_gated(2:end,:), 2);
            % test2 = obj.listPG1;
            % toc
            % tic
            % new 0.162 seconds
            fprintf('energy gating ...')
            tic
            % 탐지기 ID 인덱스 계산 (1부터 시작하도록 조정)
            detIndices = obj.listPG_ungated(:,1) + 1;
            % 에너지 값 추출
            energy = obj.listPG_ungated(:,3);
            % 각 탐지기에 대한 에너지 윈도우의 하한과 상한을 미리 추출
            eWindowInideces = eWindow(detIndices, :);
            % 범위 내에 있는 데이터만 선택
            obj.listPG1 = obj.listPG_ungated(energy >= eWindowInideces(:,1) & energy <= eWindowInideces(:,2),:);
            toc
            fprintf('finished!\n')
        end

        function [] = splitPGintoSpots_v4_dsearchN_nnz_speedUp(obj)
            tic
            fprintf('splitting PG data into spots ...')
            % time lag correction
            obj.timeTrig2 = obj.timeTrig1*0.999977577721224; %초단위
            obj.listPG2 = obj.listPG1;
            obj.listPG2(:,2) = obj.listPG1(:,2)*0.999977577721224;
            obj.trigSignal2 = obj.trigSignal1;
            obj.trigSignal2(:,1) = obj.trigSignal1(:,1)*0.999977577721224;

            % time offset correction 여기 2.3 초
            spotMidTime_log = mean(obj.dataSet(:,1:2),2); %초 단위
            spotMidTime_trig = mean(obj.timeTrig2,2);
            idx_mainLayer = find(obj.dataSet(:,4)==0);
            timeRefLog = mean(obj.dataSet(idx_mainLayer(1),1:2),2); %첫 번째 레이어 첫 스팟 시간
            sumTimeDiff = ones(size(spotMidTime_trig, 1), 1)*1000000;
            for ii = 1:size(spotMidTime_trig, 1)
                tempTimeLag = spotMidTime_trig(ii) - timeRefLog;
                spotMidTime_trig_temp = spotMidTime_trig - tempTimeLag;
                idx_trig = knnsearch(spotMidTime_trig_temp,spotMidTime_log);
                timeDiff = spotMidTime_trig_temp(idx_trig) - spotMidTime_log;
                sumTimeDiff(ii) = sum(abs(timeDiff));
            end
            [minVal, minIdx] = min(sumTimeDiff);
            timeLag = spotMidTime_trig(minIdx)-timeRefLog;
            fprintf(['Average time error in matching spots (trigger vs log) is ', num2str(minVal/size(spotMidTime_log, 1), '%.3f'), ' seconds \n'] );
            obj.timeTrig = obj.timeTrig2 - timeLag;
            obj.listPG = obj.listPG2;
            obj.listPG(:,2) = obj.listPG2(:,2) - timeLag;
            obj.trigSignal = obj.trigSignal2;
            obj.trigSignal(:,1) = obj.trigSignal2(:,1) - timeLag;

            % trig log matching 여기 1 초
            
            obj.timeTrig_matched = zeros(length(obj.dataSet),2);
            trigPool = obj.timeTrig;
            for idxLog = 1:length(obj.dataSet)
                chk = 0;
                for idxTrig = 1:size(trigPool,1)
                    meanTrig = mean(trigPool(idxTrig,:),2);
                    meanLog = mean(obj.dataSet(idxLog,1:2),2);
                    if abs(meanTrig-meanLog) < 0.05 % 50 ms 이내이면
                        obj.timeTrig_matched(idxLog,:) =  trigPool(idxTrig,:);
                        trigPool(idxTrig,:)=[];
                        chk=1;
                        break;
                    end
                end
                if ~chk
                    warning('log spot was not matched with trigger spot')
                end
            end

            EventTimes = obj.listPG(:, 2);
            EventDetectors = obj.listPG(:, 1);
            IntervalStarts = obj.timeTrig_matched(:, 1);
            IntervalEnds = obj.timeTrig_matched(:, 2);
            numTriggers = length(IntervalStarts);
            numDetectors = length(obj.numDets);

            % 이벤트와 트리거 시간을 정렬
            [EventTimes, sortIdx] = sort(EventTimes);
            EventDetectors = EventDetectors(sortIdx);

            % 이벤트를 해당 트리거 인터벌에 매핑
            EventIntervalIndices = zeros(size(EventTimes));
            eventIdx = 1;

            for iiTrig = 1:numTriggers
                % 현재 트리거의 시간 범위 가져오기
                timeStart = IntervalStarts(iiTrig);
                timeEnd = IntervalEnds(iiTrig);

                % 이벤트 시간이 트리거 시간 범위 내에 있는지 확인
                while eventIdx <= length(EventTimes) && EventTimes(eventIdx) < timeStart
                    eventIdx = eventIdx + 1;
                end
                startIdx = eventIdx;

                while eventIdx <= length(EventTimes) && EventTimes(eventIdx) <= timeEnd
                    EventIntervalIndices(eventIdx) = iiTrig;
                    eventIdx = eventIdx + 1;
                end
            end

            % 유효한 이벤트만 선택
            validEvents = EventIntervalIndices > 0;
            validIntervals = EventIntervalIndices(validEvents);
            validDetectors = EventDetectors(validEvents);

            % 검출기 인덱스를 obj.numDets의 위치로 매핑
            [isMember, detIndices] = ismember(validDetectors, obj.numDets);
            validDetectors = detIndices(isMember);
            validIntervals = validIntervals(isMember);

            % 발생 횟수 계산
            subs = [validIntervals, validDetectors];
            counts = accumarray(subs, 1, [numTriggers, numDetectors]);

            % 결과 저장
            obj.spotPG_unsorted = counts;

            midTimeTrig = mean(obj.timeTrig_matched, 2);
            midTimeLog = mean(obj.dataSet(:, 1:2), 2);

            % plotting
            figure;hold on; box on; grid on;
            yyaxis left
            plotTimeStructure(obj.dataSet(:,1:2), 'log', 'b');
            plotTimeStructure(obj.timeTrig(:,1:2), 'trig', 'g');
            yyaxis right
            binWidth = 0.0001; %0.1ms
            [sumPGcount, edges] = histcounts(obj.listPG(:,2), 0:binWidth:max(obj.listPG(:,2)));
            bar(edges(1:end-1), sumPGcount,'LineWidth', 1, 'EdgeColor','k','FaceColor','k', 'DisplayName', 'count rate');
            title('time structure');
            ylabel('count per 0.1 ms)');
            yyaxis left

            scatter(midTimeLog, (midTimeTrig-midTimeLog)*1E3, 10, 'filled', "o", 'DisplayName','pg - log');
            ylabel('time difference (ms)');
            xlabel('time elapsed (s)')
            legend('Location','northeastoutside');
            ylim([-10 120]);
            % set(gca, 'YLim',[-20 120]);
            hold off;
            fprintf('finished!')
            toc

        end

        function [] = splitPGintoSpots_v3_nnz_speedUp(obj)
            tic
            fprintf('splitting PG data into spots ...')
            % time lag correction
            obj.timeTrig2 = obj.timeTrig1*0.999977577721224; %초단위
            obj.listPG2 = obj.listPG1;
            obj.listPG2(:,2) = obj.listPG1(:,2)*0.999977577721224;
            obj.trigSignal2 = obj.trigSignal1;
            obj.trigSignal2(:,1) = obj.trigSignal1(:,1)*0.999977577721224;

            % time offset correction 여기 2.3 초
            spotMidTime_log = mean(obj.dataSet(:,1:2),2); %초 단위
            spotMidTime_trig = mean(obj.timeTrig2,2);
            idx_mainLayer = find(obj.dataSet(:,4)==0);
            timeRefLog = mean(obj.dataSet(idx_mainLayer(1),1:2),2); %첫 번째 레이어 첫 스팟 시간
            sumTimeDiff = ones(size(spotMidTime_trig, 1), 1)*1000000;
            for ii = 1:size(spotMidTime_trig, 1)
                tempTimeLag = spotMidTime_trig(ii) - timeRefLog;
                spotMidTime_trig_temp = spotMidTime_trig - tempTimeLag;
                idx_trig = knnsearch(spotMidTime_trig_temp,spotMidTime_log);
                timeDiff = spotMidTime_trig_temp(idx_trig) - spotMidTime_log;
                sumTimeDiff(ii) = sum(abs(timeDiff));
            end
            [minVal, minIdx] = min(sumTimeDiff);
            timeLag = spotMidTime_trig(minIdx)-timeRefLog;
            fprintf(['Average time error in matching spots (trigger vs log) is ', num2str(minVal/size(spotMidTime_log, 1), '%.3f'), ' seconds \n'] );
            obj.timeTrig = obj.timeTrig2 - timeLag;
            obj.listPG = obj.listPG2;
            obj.listPG(:,2) = obj.listPG2(:,2) - timeLag;
            obj.trigSignal = obj.trigSignal2;
            obj.trigSignal(:,1) = obj.trigSignal2(:,1) - timeLag;

            % trig log matching 여기 1 초
            
            obj.timeTrig_matched = zeros(length(obj.dataSet),2);
            trigPool = obj.timeTrig;
            for idxLog = 1:length(obj.dataSet)
                chk = 0;
                for idxTrig = 1:size(trigPool,1)
                    meanTrig = mean(trigPool(idxTrig,:),2);
                    meanLog = mean(obj.dataSet(idxLog,1:2),2);
                    if abs(meanTrig-meanLog) < 0.05 % 50 ms 이내이면
                        obj.timeTrig_matched(idxLog,:) =  trigPool(idxTrig,:);
                        trigPool(idxTrig,:)=[];
                        chk=1;
                        break;
                    end
                end
                if ~chk
                    warning('log spot was not matched with trigger spot')
                end
            end

            % splitting ver 4 1 초

            iiStart = 1;
            iiEnd = 1;
            numTriggers = size(obj.timeTrig_matched, 1);
            numDetectors = length(obj.numDets);
            obj.spotPG_unsorted = zeros(numTriggers, numDetectors);

            for iiTrig = 1:numTriggers
                timeStart = obj.timeTrig_matched(iiTrig, 1);
                timeEnd = obj.timeTrig_matched(iiTrig, 2);
                iiStart = dsearchn(obj.listPG(iiStart:end, 2), timeStart) + iiStart - 1;
                iiEnd = dsearchn(obj.listPG(iiEnd:end, 2), timeEnd) + iiEnd - 1;

                % 시간 구간 내의 검출기 데이터 추출
                detector_data = obj.listPG(iiStart:iiEnd, 1);

                % 검출기 인덱스를 obj.numDets의 위치로 매핑
                [isMember, det_idx] = ismember(detector_data, obj.numDets);

                % obj.numDets에 포함된 검출기만 필터링
                valid_det_idx = det_idx(isMember);

                % accumarray를 사용하여 발생 횟수 카운트
                counts = accumarray(valid_det_idx, 1, [numDetectors, 1]);

                % 해당 행에 카운트 할당
                obj.spotPG_unsorted(iiTrig, :) = counts';
            end

            midTimeTrig = mean(obj.timeTrig_matched(:, :), 2);
            midTimeLog = mean(obj.dataSet(:, 1:2), 2);

            % plotting
            figure;hold on; box on; grid on;
            yyaxis left
            plotTimeStructure(obj.dataSet(:,1:2), 'log', 'b');
            plotTimeStructure(obj.timeTrig(:,1:2), 'trig', 'g');
            yyaxis right
            binWidth = 0.0001; %0.1ms
            [sumPGcount, edges] = histcounts(obj.listPG(:,2), 0:binWidth:max(obj.listPG(:,2)));
            bar(edges(1:end-1), sumPGcount,'LineWidth', 1, 'EdgeColor','k','FaceColor','k', 'DisplayName', 'count rate');
            title('time structure');
            ylabel('count per 0.1 ms)');
            yyaxis left

            scatter(midTimeLog, (midTimeTrig-midTimeLog)*1E3, 10, 'filled', "o", 'DisplayName','pg - log');
            ylabel('time difference (ms)');
            xlabel('time elapsed (s)')
            legend('Location','northeastoutside');
            ylim([-10 120]);
            % set(gca, 'YLim',[-20 120]);
            hold off;
            fprintf('finished!')
            toc
        end

        function [] = splitPGintoSpots_v5_optimized(obj) %% 3.76초
            total_tic = tic;
            fprintf('splitting PG data into spots ...\n');

            % 시간 지연 보정
            obj.timeTrig2 = obj.timeTrig1 * 0.999977577721224; % 초 단위
            obj.listPG2 = obj.listPG1;
            obj.listPG2(:, 2) = obj.listPG1(:, 2) * 0.999977577721224;
            obj.trigSignal2 = obj.trigSignal1;
            obj.trigSignal2(:, 1) = obj.trigSignal1(:, 1) * 0.999977577721224;

            % 시간 오프셋 보정
            spotMidTime_log = mean(obj.dataSet(:, 1:2), 2); % 초 단위
            spotMidTime_trig = mean(obj.timeTrig2, 2);
            idx_mainLayer = find(obj.dataSet(:, 4) == 0);
            timeRefLog = mean(obj.dataSet(idx_mainLayer(1), 1:2), 2); % 첫 번째 레이어 첫 스팟 시간
            sumTimeDiff = ones(size(spotMidTime_trig, 1), 1) * 1e6;
            for ii = 1:size(spotMidTime_trig, 1)
                tempTimeLag = spotMidTime_trig(ii) - timeRefLog;
                spotMidTime_trig_temp = spotMidTime_trig - tempTimeLag;
                idx_trig = knnsearch(spotMidTime_trig_temp, spotMidTime_log);
                timeDiff = spotMidTime_trig_temp(idx_trig) - spotMidTime_log;
                sumTimeDiff(ii) = sum(abs(timeDiff));
            end
            [minVal, minIdx] = min(sumTimeDiff);
            timeLag = spotMidTime_trig(minIdx) - timeRefLog;
            fprintf('Average time error in matching spots (trigger vs log) is %.3f seconds \n', minVal / size(spotMidTime_log, 1));
            obj.timeTrig = obj.timeTrig2 - timeLag;
            obj.listPG = obj.listPG2;
            obj.listPG(:, 2) = obj.listPG2(:, 2) - timeLag;
            obj.trigSignal = obj.trigSignal2;
            obj.trigSignal(:, 1) = obj.trigSignal2(:, 1) - timeLag;

            % 트리거 로그 매칭
            obj.timeTrig_matched = zeros(length(obj.dataSet), 2);
            trigPool = obj.timeTrig;
            for idxLog = 1:length(obj.dataSet)
                chk = 0;
                for idxTrig = 1:size(trigPool, 1)
                    meanTrig = mean(trigPool(idxTrig, :), 2);
                    meanLog = mean(obj.dataSet(idxLog, 1:2), 2);
                    if abs(meanTrig - meanLog) < 0.05 % 50 ms 이내이면
                        obj.timeTrig_matched(idxLog, :) = trigPool(idxTrig, :);
                        trigPool(idxTrig, :) = [];
                        chk = 1;
                        break;
                    end
                end
                if ~chk
                    warning('log spot was not matched with trigger spot');
                end
            end

            % 데이터 분할 및 연산 속도 개선
            event_times = obj.listPG(:, 2);
            event_detectors = obj.listPG(:, 1);
            num_events = length(event_times);
            num_intervals = size(obj.timeTrig_matched, 1);
            obj.spotPG_unsorted = zeros(num_intervals, length(obj.numDets));
            event_idx = 1;

            for interval_idx = 1:num_intervals
                timeStart = obj.timeTrig_matched(interval_idx, 1);
                timeEnd = obj.timeTrig_matched(interval_idx, 2);

                % 이벤트 인덱스 업데이트 (이벤트 시간과 인터벌 시간 비교)
                while event_idx <= num_events && event_times(event_idx) < timeStart
                    event_idx = event_idx + 1;
                end
                event_idx_start = event_idx;

                while event_idx <= num_events && event_times(event_idx) <= timeEnd
                    event_idx = event_idx + 1;
                end
                event_idx_end = event_idx - 1;

                % 인터벌 내의 이벤트 처리
                if event_idx_end >= event_idx_start
                    detectors_in_interval = event_detectors(event_idx_start:event_idx_end);
                    for idx_det = obj.numDets % 스팟 구간 내의 계수값 정리
                        obj.spotPG_unsorted(interval_idx, idx_det + 1) = sum(detectors_in_interval == idx_det);
                    end
                end
            end

            % 그래프 출력 (필요한 경우)
            % ... (생략) ...

            fprintf('finished!\n');
            toc(total_tic);
        end

        
        function [] = splitPGintoSpots_v2(obj)
            tic
            fprintf('splitting PG data into spots ...')
            % time lag correction
            obj.timeTrig2 = obj.timeTrig1*0.999977577721224; %초단위
            obj.listPG2 = obj.listPG1;
            obj.listPG2(:,2) = obj.listPG1(:,2)*0.999977577721224;
            obj.trigSignal2 = obj.trigSignal1;
            obj.trigSignal2(:,1) = obj.trigSignal1(:,1)*0.999977577721224;

            % time offset correction 여기 2.3 초
            spotMidTime_log = mean(obj.dataSet(:,1:2),2); %초 단위
            spotMidTime_trig = mean(obj.timeTrig2,2);
            idx_mainLayer = find(obj.dataSet(:,4)==0);
            timeRefLog = mean(obj.dataSet(idx_mainLayer(1),1:2),2); %첫 번째 레이어 첫 스팟 시간
            sumTimeDiff = ones(size(spotMidTime_trig, 1), 1)*1000000;
            for ii = 1:size(spotMidTime_trig, 1)
                tempTimeLag = spotMidTime_trig(ii) - timeRefLog;
                spotMidTime_trig_temp = spotMidTime_trig - tempTimeLag;
                idx_trig = knnsearch(spotMidTime_trig_temp,spotMidTime_log);
                timeDiff = spotMidTime_trig_temp(idx_trig) - spotMidTime_log;
                sumTimeDiff(ii) = sum(abs(timeDiff));
            end
            [minVal, minIdx] = min(sumTimeDiff);
            timeLag = spotMidTime_trig(minIdx)-timeRefLog;
            fprintf(['Average time error in matching spots (trigger vs log) is ', num2str(minVal/size(spotMidTime_log, 1), '%.3f'), ' seconds \n'] );
            obj.timeTrig = obj.timeTrig2 - timeLag;
            obj.listPG = obj.listPG2;
            obj.listPG(:,2) = obj.listPG2(:,2) - timeLag;
            obj.trigSignal = obj.trigSignal2;
            obj.trigSignal(:,1) = obj.trigSignal2(:,1) - timeLag;

            % trig log matching 여기 1 초
            
            obj.timeTrig_matched = zeros(length(obj.dataSet),2);
            trigPool = obj.timeTrig;
            for idxLog = 1:length(obj.dataSet)
                chk = 0;
                for idxTrig = 1:size(trigPool,1)
                    meanTrig = mean(trigPool(idxTrig,:),2);
                    meanLog = mean(obj.dataSet(idxLog,1:2),2);
                    if abs(meanTrig-meanLog) < 0.05 % 50 ms 이내이면
                        obj.timeTrig_matched(idxLog,:) =  trigPool(idxTrig,:);
                        trigPool(idxTrig,:)=[];
                        chk=1;
                        break;
                    end
                end
                if ~chk
                    warning('log spot was not matched with trigger spot')
                end
            end

            % splitting ver 4 1 초
            iiStart = 1;
            iiEnd = 1;
            obj.spotPG_unsorted = zeros(length(obj.timeTrig_matched), length(obj.numDets));
            for iiTrig = 1:size(obj.timeTrig_matched,1)
                timeStart = obj.timeTrig_matched(iiTrig,1);
                timeEnd = obj.timeTrig_matched(iiTrig,2);
                iiStart = dsearchn(obj.listPG(iiStart:end,2), timeStart)+iiStart-1;
                iiEnd = dsearchn(obj.listPG(iiEnd:end,2), timeEnd)+iiEnd-1;
                for idx_det = obj.numDets %스팟 구간 내의 계수값 정리
                    obj.spotPG_unsorted(iiTrig, idx_det+1) = nnz(obj.listPG(iiStart:iiEnd,1)==idx_det);
                end
                %                 disp(iiTrig)
            end
            % for iiTrig = 1:size(obj.timeTrig_matched,1)
            %     timeStart = obj.timeTrig_matched(iiTrig,1);
            %     timeEnd = obj.timeTrig_matched(iiTrig,2);
            %     iiStart = bns(obj.listPG, 2, timeStart, iiStart)-1;
            %     iiEnd = bns(obj.listPG,2 , timeEnd, iiEnd)-1;
            %     for idx_det = obj.numDets %스팟 구간 내의 계수값 정리
            %         obj.spotPG_unsorted(iiTrig, idx_det+1) = nnz(obj.listPG(iiStart:iiEnd,1)==idx_det);
            %     end
            %     %                 disp(iiTrig)
            % end
            midTimeTrig = mean(obj.timeTrig_matched(:,:),2);
            midTimeLog = mean(obj.dataSet(:,1:2),2);

            % plotting
            figure;hold on; box on; grid on;
            yyaxis left
            plotTimeStructure(obj.dataSet(:,1:2), 'log', 'b');
            plotTimeStructure(obj.timeTrig(:,1:2), 'trig', 'g');
            yyaxis right
            binWidth = 0.0001; %0.1ms
            [sumPGcount, edges] = histcounts(obj.listPG(:,2), 0:binWidth:max(obj.listPG(:,2)));
            bar(edges(1:end-1), sumPGcount,'LineWidth', 1, 'EdgeColor','k','FaceColor','k', 'DisplayName', 'count rate');
            title('time structure');
            ylabel('count per 0.1 ms)');
            yyaxis left

            scatter(midTimeLog, (midTimeTrig-midTimeLog)*1E3, 10, 'filled', "o", 'DisplayName','pg - log');
            ylabel('time difference (ms)');
            xlabel('time elapsed (s)')
            legend('Location','northeastoutside');
            ylim([-10 120]);
            % set(gca, 'YLim',[-20 120]);
            hold off;
            fprintf('finished!')
            toc

        end

        function [] = splitPGintoSpots(obj)
            tic
            fprintf('splitting PG data into spots ...')
            % time lag correction
            obj.timeTrig2 = obj.timeTrig1*0.999977577721224; %초단위
            obj.listPG2 = obj.listPG1;
            obj.listPG2(:,2) = obj.listPG1(:,2)*0.999977577721224;
            obj.trigSignal2 = obj.trigSignal1;
            obj.trigSignal2(:,1) = obj.trigSignal1(:,1)*0.999977577721224;

            % time offset correction
            spotMidTime_log = mean(obj.dataSet(:,1:2),2); %초 단위
            for idxTrig = 1:length(obj.timeTrig2)
                if (obj.timeTrig2(idxTrig+15) - obj.timeTrig2(idxTrig)) < 0.5 %현재 스팟으로부터 10개 뒤 스팟까지 시간 소요가 0.5초 미만이면
                    offset_trig = obj.timeTrig2(idxTrig,1); %초단위
                    break;
                end
            end
            for idx_log = 1:length(obj.dataSet)
                if (spotMidTime_log(idx_log+10) - spotMidTime_log(idx_log)) < 0.5 %현재 스팟으로부터 10개 뒤 스팟까지 시간 소요가 0.5초 미만이면
                    offset_log = spotMidTime_log(idx_log,1);
                    break;
                end
            end
            timeLag = offset_log - offset_trig;
            obj.timeTrig = obj.timeTrig2 + timeLag;
            obj.listPG = obj.listPG2;
            obj.listPG(:,2) = obj.listPG2(:,2) + timeLag;
            obj.trigSignal = obj.trigSignal2;
            obj.trigSignal(:,1) = obj.trigSignal2(:,1) + timeLag;

            % trig log matching
            obj.timeTrig_matched = zeros(length(obj.dataSet),2);
            trigPool = obj.timeTrig;
            for idxLog = 1:length(obj.dataSet)
                chk = 0;
                for idxTrig = 1:size(trigPool,1)
                    meanTrig = mean(trigPool(idxTrig,:),2);
                    meanLog = mean(obj.dataSet(idxLog,1:2),2);
                    if abs(meanTrig-meanLog) < 0.05 % 50 ms 이내이면
                        obj.timeTrig_matched(idxLog,:) =  trigPool(idxTrig,:);
                        trigPool(idxTrig,:)=[];
                        chk=1;
                        break;
                    end
                end
                if ~chk
                    warning('log spot was not matched with trigger spot')
                end
            end

            % splitting ver 4
            iiStart = 1;
            iiEnd = 1;
            obj.spotPG_unsorted = zeros(length(obj.timeTrig_matched), length(obj.numDets));
            for iiTrig = 1:size(obj.timeTrig_matched,1)
                timeStart = obj.timeTrig_matched(iiTrig,1);
                timeEnd = obj.timeTrig_matched(iiTrig,2);
                iiStart = dsearchn(obj.listPG(iiStart:end,2), timeStart)+iiStart-1;
                iiEnd = dsearchn(obj.listPG(iiEnd:end,2), timeEnd)+iiEnd-1;
                for idx_det = obj.numDets %스팟 구간 내의 계수값 정리
                    obj.spotPG_unsorted(iiTrig, idx_det+1) = nnz(obj.listPG(iiStart:iiEnd,1)==idx_det);
                end
                %                 disp(iiTrig)
            end

            % splitting ver 3
            %             obj.spotPG_unsorted = zeros(length(obj.timeTrig_matched), length(obj.numDets));
            %             for iiTrig = 1:size(obj.timeTrig_matched,1)
            %                 timeStart = obj.timeTrig_matched(iiTrig,1);
            %                 timeEnd = obj.timeTrig_matched(iiTrig,2);
            %                 iiStart = dsearchn(obj.listPG(:,2), timeStart);
            %                 iiEnd = dsearchn(obj.listPG(:,2), timeEnd);
            %                 for idx_det = obj.numDets %스팟 구간 내의 계수값 정리
            %                     obj.spotPG_unsorted(idxTrig, idx_det+1) = nnz(obj.listPG(iiStart:iiEnd,1)==idx_det);
            %                 end
            %                 disp(iiTrig)
            %             end

            %             % splitting ver 2
            %             obj.spotPG_unsorted = zeros(length(obj.timeTrig_matched), length(obj.numDets));
            %             for iiTrig = 1:size(obj.timeTrig_matched,1)
            %                 timeStart = obj.timeTrig_matched(iiTrig,1);
            %                 timeEnd = obj.timeTrig_matched(iiTrig,2);
            %                 idxSpot = obj.listPG(:,2) > timeStart & obj.listPG(:,2) < timeEnd;
            %                 for idx_det = obj.numDets %스팟 구간 내의 계수값 정리
            %                     obj.spotPG_unsorted(idxTrig, idx_det+1) = nnz(obj.listPG(idxSpot,1)==idx_det);
            %                 end
            %                 disp(iiTrig)
            %             end

            % splitting ver 1
            %             obj.spotPG_unsorted = zeros(length(obj.timeTrig_matched), length(obj.numDets));
            %             idxTrig = 1;
            %             state = 0;
            %             for idxPG = 1:length(obj.listPG) %루프 pg list
            %                 if state==0 % 스팟 OFF 상태에서
            %                     if obj.listPG(idxPG,2) > obj.timeTrig_matched(idxTrig,1) %현재 pg line의 시간이 trig on 시간 이후이면
            %                         idxPG_left = idxPG; %스팟의 왼쪽은 현재 pg line이다
            %                         state = 1; % ON으로 전환
            %                     end
            %                 elseif state == 1 % 스팟 ON 상태에서
            %                     if obj.listPG(idxPG,2) > obj.timeTrig_matched(idxTrig,2) %현재 pg line의 시간이 trig off 시간 이후이면
            %                         idxPG_right = idxPG-1; %스팟의 오른쪽은 현재 pg line이다
            %                         for idx_det = obj.numDets %스팟 구간 내의 계수값 정리
            %                             obj.spotPG_unsorted(idxTrig, idx_det+1) = nnz(obj.listPG(idxPG_left:idxPG_right,1)==idx_det);
            %                         end
            %                         state = 0; % OFF로 전환
            %                         idxTrig = idxTrig+1; % 다음 트리거로 전환
            %                         if idxTrig > length(obj.timeTrig_matched) %트리거 끝나면 for문 종료
            %                             break
            %                         end
            %                     end
            %                 end
            % %                                 disp(idxPG)
            %             end
            %-----------------------------------------



            midTimeTrig = mean(obj.timeTrig_matched(:,:),2);
            midTimeLog = mean(obj.dataSet(:,1:2),2);

            % plotting
            figure;hold on; box on; grid on;
            yyaxis left
            plotTimeStructure(obj.dataSet(:,1:2), 'log', 'b');
            plotTimeStructure(obj.timeTrig(:,1:2), 'trig', 'g');
            yyaxis right
            binWidth = 0.0001; %0.1ms
            [sumPGcount, edges] = histcounts(obj.listPG(:,2), 0:binWidth:max(obj.listPG(:,2)));
            bar(edges(1:end-1), sumPGcount,'LineWidth', 1, 'EdgeColor','k','FaceColor','k', 'DisplayName', 'count rate');
            title('time structure');
            ylabel('count per 0.1 ms)');
            yyaxis left

            plot(midTimeLog, (midTimeTrig-midTimeLog)*1E3, '.-r', 'DisplayName','pg - log', 'LineWidth',2, 'MarkerSize',10);
            ylabel('time difference (ms)');
            xlabel('time elapsed (s)')
            legend('Location','northeastoutside');
            ylim([-20 120]);
            set(gca, 'YLim',[-20 120]);
            hold off;
            fprintf('finished!')
            toc

        end

        function [] = sortPGdist(obj)
            tic
            fprintf('sorting PG distribution ...')
            len = length(obj.spotPG_unsorted);
            obj.spotPG = zeros(len,72);
            obj.spotPG (:, 1:2:35) = obj.spotPG_unsorted(:, 37:54) + obj.spotPG_unsorted(:, 55:72);
            obj.spotPG (:, 2:2:36) = obj.spotPG_unsorted(:, 1:18) + obj.spotPG_unsorted(:, 19:36);
            obj.spotPG (:, 37:2:71) = obj.spotPG_unsorted(:, 109:126) + obj.spotPG_unsorted(:, 127:144);
            obj.spotPG (:, 38:2:72) = obj.spotPG_unsorted(:, 73:90) + obj.spotPG_unsorted(:, 91:108);
            obj.spotPGmm = (obj.spotPG (:, 1:end-1) + obj.spotPG (:, 2:end))/2;
            fprintf('finished!')
            toc
        end

        function [] = showTotalSlitCount(obj)
            sumSlitCount = sum(obj.spotPG_unsorted,1);
            figure('Position',[500 200 600 1000]);
            subplot(4,1,1); hold on; box on; grid on;
            bar(0:35, [sumSlitCount(1:18), sumSlitCount(73:90)], 1, 'EdgeColor','none','FaceColor',[0.3 0.3 0.3],'LineWidth',1E-10);
            xticks(0:35)
            xticklabels({'0','1','2','3','4','5','6','7','8','9','10','11','12','13','14','15','16','17','72','73','74','75','76','77','78','79','80','81','82','83','84','85','86','87','88','89'})
            set(gca, 'FontSize', 12, 'FontWeight', 'bold')
            hold off
            subplot(4,1,2); hold on; box on; grid on;
            bar(0:35, [sumSlitCount(19:36), sumSlitCount(91:108)], 1, 'EdgeColor','none','FaceColor',[0.3 0.3 0.3],'LineWidth',1E-10);
            xticks(0:35)
            xticklabels({'18','19','20','21','22','23','24','25','26','27','28','29','30','31','32','33','34','35','90','91','92','93','94','95','96','97','98','99','100','101','102','103','104','105','106','107'});
            set(gca, 'FontSize', 12, 'FontWeight', 'bold')
            hold off
            subplot(4,1,3); hold on; box on; grid on;
            bar(0:35, [sumSlitCount(37:54), sumSlitCount(109:126)], 1, 'EdgeColor','none','FaceColor',[0.3 0.3 0.3],'LineWidth',1E-10);
            xticks(0:35)
            xticklabels({'36','37','38','39','40','41','42','43','44','45','46','47','48','49','50','51','52','53','108','109','110','111','112','113','114','115','116','117','118','119','120','121','122','123','124','125'});
            set(gca, 'FontSize', 12, 'FontWeight', 'bold')
            hold off
            subplot(4,1,4); hold on; box on; grid on;
            bar(0:35, [sumSlitCount(55:72), sumSlitCount(127:144)], 1, 'EdgeColor','none','FaceColor',[0.3 0.3 0.3],'LineWidth',1E-10);
            xticks(0:35)
            xticklabels({'54','55','56','57','58','59','60','61','62','63','64','65','66','67','68','69','70','71','126','127','128','129','130','131','132','133','134','135','136','137','138','139','140','141','142','143'});
            set(gca, 'FontSize', 12, 'FontWeight', 'bold')
            hold off
        end

        function [] = showTotalPGdist(obj)
            figure; hold on; box on; grid on;
            bar(obj.gridFoVmm, sum(obj.spotPGmm,1) , 1, 'EdgeColor','none','FaceColor',[0.3 0.3 0.3],'LineWidth',1E-10, 'DisplayName', 'count dist. (movemean)');
            ylim([min(sum(obj.spotPGmm,1)) inf])
            ylabel('count');
            xlabel('location (mm)');
            set(gca, 'FontSize', 12, 'FontWeight', 'bold')
            hold off
        end

        function obj = setGridDepth(obj, cameraOffset, cameraDepth)
            obj.cameraOffset = cameraOffset;
            obj.cameraDepth = cameraDepth;
            obj.gridFoVmm = obj.gridFoVmm+cameraOffset;
            obj.gridDepth = obj.gridFoVmm+cameraDepth+cameraOffset;
        end
        function obj = getTrueRangePMMA(obj, rangePMMA80)
            dataSet_temp = obj.dataSet;
            for ii=1:length(dataSet_temp)
                trueRange = rangePMMA80(dsearchn(rangePMMA80(:,1),dataSet_temp(ii,10)),2);
                zPlan = trueRange-obj.cameraDepth;
                dataSet_temp(ii,13) = zPlan;
                dataSet_temp(ii,17) = trueRange;
            end
            obj.dataSet = dataSet_temp;
        end

        function obj = setTrueRange(obj, gapPeakRange)
            tic
            fprintf('setting true range by gap peak range water ...')
            for ii=1:length(obj.dataSet)
                gap = gapPeakRange(dsearchn(gapPeakRange(:,1),obj.dataSet(ii,10)),2);
                plannedRange = obj.dataSet(ii,13) + gap;
                obj.dataSet(ii,17) = plannedRange;
            end
            fprintf('finished!')
            toc
        end

        function obj = aggregate3dGaussian(obj, sigma)
            tic
            [obj.dataSet_agg, obj.spotPG_agg]  = agg_spotwise_3Dgauss_v5(obj.gridFoVmm, obj.dataSet, obj.spotPGmm, sigma, obj.beamDir);
            % agg_spotwise_3Dgauss_v5
            toc
            obj.dataSet_agg = obj.dataSet_agg(obj.dataSet_agg(:,14)~=0,:); % match 안된 스팟 배제
            obj.spotPG_agg = obj.spotPG_agg(obj.dataSet_agg(:,14)~=0,:); % match 안된 스팟 배제
        end

        

    end

end

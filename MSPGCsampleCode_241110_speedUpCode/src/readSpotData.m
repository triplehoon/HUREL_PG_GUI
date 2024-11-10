function [spotData] = readSpotData(file_pld,file_3dpos)
% pld file 및 spot position 파일을 선택하면, spot data 출력함.
% spot data는 레이어번호, X, Y, Z, MU 순으로 출력함.

% path_dataFiles = 'G:\공유 드라이브\MSPGC\04_Experiment\210326_NCC_RangeMeasurementForPlanBeam\PlanBeamAnalysis\dataFiles';
fid_pld = fopen(string(file_pld));
tline = fgetl(fid_pld);
header = split(tline,',');
totalMU = str2double(header{8});
cumMeterSetWeight = str2double(header{9});
numLayer = str2double(header{10});


%%
fid_spot3dpos = fopen(string(file_3dpos));
idx_line=1;
idx_Layer=0;
spotData = zeros(1,5);
while ~feof(fid_spot3dpos)
    tline = fgetl(fid_spot3dpos);
    scanline = textscan(tline,'%s');
    %     try
    if('L' == scanline{1,1}{1}(1)) % layer header
        C = strsplit(scanline{1,1}{1},',');
        energy = str2double(C{3});
        idx_Layer = idx_Layer+1;
    else % element
        spotData(idx_line,1) = idx_Layer;
        spotData(idx_line,2) = energy;
        spotData(idx_line,3) = str2double(scanline{1,1}{1});
        spotData(idx_line,4) = str2double(scanline{1,1}{2});
        spotData(idx_line,5) = str2double(scanline{1,1}{3});
        spotData(idx_line,6) = str2double(scanline{1,1}{4})*totalMU/cumMeterSetWeight;
        idx_line=idx_line+1;
    end
    %     catch
    %         spotData(idx_line,1) = idx_Layer;
    %         spotData(idx_line,2) = energy;
    %         spotData(idx_line,3) = str2double(scanline{1,1}{1});
    %         spotData(idx_line,4) = str2double(scanline{1,1}{2});
    %         spotData(idx_line,5) = str2double(scanline{1,1}{3});
    %         spotData(idx_line,6) = str2double(scanline{1,1}{4});
    %         idx_line=idx_line+1;
    %     end
end

end


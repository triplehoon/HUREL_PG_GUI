%% Setting
addpath('src'); % 경로에 소스코드 폴더 추가
beamDir = 1; %빔 방향 좌->우(1)
sigmaAgg = 10; % spot aggregation sigma 값 설정
load gapPeakRange.mat


%% Select Files by UI
[file_calib, path_calib] = uigetfile('*.mat', 'Select energy calibration file'); % 에너지 교정 파일 선택;
load([path_calib file_calib]); %에너지 교정 파일 불러오기
[file_pg, path_pg] = uigetfile('*.bin', 'Select PG file'); % PG 파일 선택;
logFilePath = uigetdir('Select log folder'); % LOG 폴더 선택;
[file_pld, path_pld] = uigetfile('*.pld', 'Select pld file'); % PLD 파일 선택;
[file_3dpld, path_3dpld] = uigetfile('*.pld', 'Select 3d pld file'); % 3D PLD 파일 선택;

%%
S = session(); % 세션 생성
S.eWindow = eWindow;
% S.setEnergyWindow(detectors); % 에너지 교정
S.loadDataSet([path_pg file_pg], logFilePath, [path_pld file_pld], [path_3dpld file_3dpld]);

%%
S.postProcessDataSet(0, 0, beamDir);
S.setTrueRange(gapPeakRange);


%%
S.aggregate3dGaussian(sigmaAgg);

%% debug_splitPGintoSpots_SH_1030
% splitPGintoSpots_v2 = S.dataSet_agg(:,15);

%% Spot map each layer Multiple Sessions! 세로 프랙션
meanX = mean(S.dataSet(:,11));
meanY = mean(S.dataSet(:,12));
maxR = max([abs(S.dataSet(:,11)-meanX);abs(S.dataSet(:,12)-meanY)]);
% rMap = (ceil( maxR / 5 ) * 5)*1.2; % 최고보다 20% 크게
rMap=60;
xLim = [meanX-rMap meanX+rMap];
yLim = [meanY-rMap meanY+rMap];
spotSizeNorm = 25;
numLayer = S.numLayer;
limAxis = [-10 10];

load('colormap_jetEdited2.mat');
cnt=0;
figure('Position',[ 50 50 numLayer*100 140]); hold on;
tiledlayout(1, numLayer, "TileSpacing",'none');
for idxLayer = 1:numLayer
    nexttile(idxLayer);
    data_tempLayer1 = S.dataSet_agg(S.dataSet_agg(:,9)==idxLayer,:);
    x = data_tempLayer1(:,11);
    y = data_tempLayer1(:,12);
    mu = data_tempLayer1(:,14);
    spotSize2 = sqrt(sqrt(mu))*spotSizeNorm;
    error = data_tempLayer1(:,15);
    scatter(x, y, spotSize2, error, 'filled');
    cnt = cnt+length(x);
    ylim(yLim);
    xlim(xLim);
    avgError = mean(error,'omitnan');
    colormap(colormap_jetEdited2);
    clim(limAxis);
    set(gca,'xtick',[],'ytick',[], 'XColor', 'none', 'YColor', 'none')
end
set(gca,'FontSize',12, 'FontWeight', 'bold')
hold off

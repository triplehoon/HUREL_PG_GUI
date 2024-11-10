function [range] = GetRange_ver4p3(x_grid, dist_rawPG,  range_true, direction)

%% Settings
if ~isempty(find(isnan(dist_rawPG),1))
    range = NaN;
    % warning('NaN value exist in pg dist');
    return
end


if ~direction
dist_rawPG = flip(dist_rawPG);
end
sigma_gaussFilt = 5; % gaussian filter window (mm)
cutoffLevel = 0.5; % cut-off level
offset = 0;
minPeakDistance = 10; % minimum peak distance
pitch = 3; % pitch (mm)

%% differentiation - centered finite difference
xgrid_diff = x_grid(2:end-1);
% dist_rawPG = smoothdata(dist_rawPG, 'gaussian', 4);
dist_diff = (dist_rawPG(3:end)-dist_rawPG(1:end-2))/(2*pitch);
dist_diff_unfilt = -dist_diff;
dist_diff = imgaussfilt(dist_diff_unfilt,sigma_gaussFilt/pitch);

% dist_diff = imgaussfilt(dist_diff_unfilt,sigma_gaussFilt/pitch, 'FilterSize',2*ceil(4*sigma_gaussFilt/pitch)+1);
% dist_diff = smoothdata(dist_diff_unfilt,'gaussian',sigma_gaussFilt/pitch*5);

%% differentiation distribution 상에서 peak 결정
[pks,locs] = findpeaks(dist_diff,xgrid_diff,'MinPeakDistance',minPeakDistance);
%     scope = max((range_true*0.027+1.6)*4/1.5,30); % 4 sigma 
    scope = 15;
    idx_valid = (range_true - scope < locs ) & (locs < range_true+scope);
    if any(idx_valid)
        locs = locs(idx_valid);
        pks = pks(idx_valid);
%         proms = proms(idx_valid);
    end
[val_pk,idx_max] = max(pks);
loc_pk = locs(idx_max);

%% Peak의 범위 지정
%범위 너무 넓으면 제한하는 것 추가하는것도 괜찮을것 같다.
idxs_localMin = find(islocalmin(dist_diff));
locs_localMin = xgrid_diff(idxs_localMin);
vals_localMin = dist_diff(idxs_localMin);
if any(locs_localMin < loc_pk)
    idx_locMinLeft = max(idxs_localMin(locs_localMin < loc_pk));
else
    idx_locMinLeft =  1;
end
if any(locs_localMin > loc_pk)
    idx_locMinRight= min(idxs_localMin(locs_localMin > loc_pk));
else
    idx_locMinRight = size(xgrid_diff,2);
end
loc_minLeft = xgrid_diff(idx_locMinLeft);
val_minLeft = dist_diff(idx_locMinLeft);
loc_minRight = xgrid_diff(idx_locMinRight);
val_minRight = dist_diff(idx_locMinRight);
bottomLevel = max(val_minLeft,val_minRight);  % 좌우측 중 최대를 바닥으로 함
baseLine = bottomLevel + cutoffLevel*(val_pk-bottomLevel); 
idx_peakRange = idx_locMinLeft:idx_locMinRight; % 좌우측 최소값 사이 구간을 피크 범위로 함.
%% Calculate Centroid 산출
xgrid_peak = xgrid_diff(idx_peakRange);
dist_peak = dist_diff(idx_peakRange);
dist_peak = dist_peak-baseLine;
sig_MR = 0;
sig_M = 0;
for ii=1:size(xgrid_peak,2)
    if(dist_peak(ii)>0)
        sig_MR = sig_MR + dist_peak(ii)*xgrid_peak(ii);
        sig_M = sig_M + dist_peak(ii);
    end
end
centroid = sig_MR/sig_M;

%% Offset 적용
range = centroid + offset; %offset of PG falloff from the range
 
% range가 scope를 벗어나는 경우
if (range_true - scope < range ) && (range < range_true+scope)
else
    range = nan;%nan 반환
    % plotFigure(x_grid, dist_rawPG, xgrid_diff, dist_diff, loc_pk, val_pk, locs_localMin, vals_localMin, loc_minLeft, loc_minRight, val_minLeft, val_minRight);
end

% range 값이 nan일 경우 에러 표시
if isnan(range)
    warning('nan range');
end

%% function
    function plotFigure(x_grid, dist_rawPG, xgrid_diff, dist_diff, loc_pk, val_pk, locs_localMin, vals_localMin, loc_minLeft, loc_minRight, val_minLeft, val_minRight)
        xlimRange = [min(x_grid) max(x_grid)];
        figure('Position', [500 100 800 800]);hold on; box on;
        
        subplot(2,1,1); hold on; grid on; box on;
        plot(x_grid, dist_rawPG, 'DisplayName', 'dist rawPG');
        xlim(xlimRange);
        legend;
        hold off

%         findpeaks(dist_diff,xgrid_diff, 'MinPeakDistance',minPeakDistance,'Annotate','extents');

        subplot(2,1,2); hold on; grid on; box on;
        plot(xgrid_diff,dist_diff);
        scatter(loc_pk, val_pk, 'o', 'DisplayName', 'maximum peak' );
        scatter(locs_localMin, vals_localMin, '.','DisplayName', 'local min');
        scatter([loc_minLeft,loc_minRight] , [val_minLeft,val_minRight], 'p', 'DisplayName', 'local minimum around peak (left right)' );
        xlim(xlimRange);

        legend('Location','northeast');
        hold off;
    end

%% get figure
% xlimRange = [min(x_grid) max(x_grid)];
% figure('Position', [500 100 800 800]);hold on; box on;
% 
% subplot(2,1,1); hold on; grid on; box on;
% % plot(x_grid-0.5*pitch, dist_rawPG, 'DisplayName', 'dist rawPG')
% plot(x_grid, dist_rawPG, 'DisplayName', 'dist rawPG');
% xlim(xlimRange);
% legend;
% hold off
% 
% findpeaks(dist_diff,xgrid_diff, 'MinPeakDistance',minPeakDistance,'Annotate','extents');
% 
% subplot(2,1,2); hold on; grid on; box on;
% plot(xgrid_diff,dist_diff);
% scatter(loc_pk, val_pk, 'o', 'DisplayName', 'maximum peak' );
% scatter(locs_localMin, vals_localMin, '.','DisplayName', 'local min');
% scatter([loc_minLeft,loc_minRight] , [val_minLeft,val_minRight], 'p', 'DisplayName', 'local minimum around peak (left right)' );
% xlim(xlimRange);
% 
% legend('Location','northeastoutside');
% hold off;

end

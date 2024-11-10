function [map_diff,map_pgdist, map_mu] = shiftMerge_SMC(S, xgrid,ygrid, sigma)
%UNTITLED4 이 함수의 요약 설명 위치
%   자세한 설명 위치
xgrid_mm = S.gridFoVmm;
dataSet = S.dataSet;
spotPGmm = S.spotPGmm;
cameraDepth = S.cameraDepth;
limlayer = [1 max(dataSet(:,3))];



% load gapPeakAndRange.mat;
% pitchX = xgrid(2)-xgrid(1);
% pitchY = ygrid(2)-ygrid(1);

map_diff = NaN(size(xgrid,2), size(ygrid,2));
map_mu = zeros(size(xgrid,2), size(ygrid,2));
map_pgdist = zeros(size(xgrid,2), size(ygrid,2), size(xgrid_mm,2));
% figure;hold on; box on;
flag_grid = zeros(size(xgrid,2), size(ygrid,2));
for idx_x = 1:size(xgrid,2)
    for idx_y = 1:size(ygrid,2)
        for idx_line = 1:size(dataSet,1)
            xpos_line = dataSet(idx_line,7);
            ypos_line = dataSet(idx_line,8);
            flag_x =  (xgrid(idx_x) - sigma < xpos_line) && (xgrid(idx_x) + sigma > xpos_line);
            flag_y =  (ygrid(idx_y) - sigma < ypos_line) && (ygrid(idx_y) + sigma > ypos_line);
            if flag_x && flag_y
                flag_grid(idx_x,idx_y) = 1;
                break;
            end
        end
    end
end

for idx_x = 1:size(xgrid,2)
    fprintf( ['Merging progress: ' num2str(idx_x) '/' num2str(size(xgrid,2)) '\n']);
    for idx_y = 1:size(ygrid,2)
        if flag_grid(idx_x,idx_y)
            dist_shiftSum = zeros(size(xgrid_mm,2),1);
            for idx_line = 1:size(dataSet,1)
                if (dataSet(idx_line, 3) <= limlayer(2)) && (dataSet(idx_line, 3) >= limlayer(1))
                xpos_line = dataSet(idx_line,7);
                ypos_line = dataSet(idx_line,8);
                distance = sqrt( (xgrid(idx_x)-xpos_line)^2 +  (ygrid(idx_y)-ypos_line)^2 );
                if distance > 3*sigma
                    continue;
                end
                weight = exp(-0.5*(distance/sigma)^2); %1/(sigma*sqrt(2*pi))*exp(-0.5*(distance/sigma)^2);
%                 energy = dataSet(idx_line,5);
                range = dataSet(idx_line,17); 
                

                xgrid_temp = xgrid_mm - (range - cameraDepth); 
                dist_shift = interp1(xgrid_temp,spotPGmm(idx_line,:),xgrid_mm)';
                dist_shift = fillmissing(dist_shift, 'nearest');
%                 dist_shift = dist_shift./max(dist_shift)*dataset_spot(idx_line, 14);
                
                dist_shift = dist_shift*weight;
                dist_shiftSum = dist_shiftSum+dist_shift;
                map_mu(idx_x,idx_y) = map_mu(idx_x,idx_y) + weight*dataSet(idx_line, 14);
%                 map_mu(idx_x,idx_y) = map_mu(idx_x,idx_y) + dataset_spot(idx_line, 14);

                %             end
                end
            end
            map_pgdist(idx_x, idx_y, :) = dist_shiftSum;
            if sum(squeeze(map_pgdist(idx_x, idx_y, :))) == 0
                map_diff(idx_x,idx_y) = NaN;
%             elseif map_mu(idx_x,idx_y) < 5
%                 map_diff(idx_x,idx_y) = NaN;
            else
                map_diff(idx_x,idx_y) = GetRange_ver4p3(xgrid_mm, squeeze(map_pgdist(idx_x, idx_y, :)),0, S.beamDir);
            end
        end
    end
end
% xlim([-70 70]); ylim([-70 70]);
% load('customColormap.mat');
% colorbar();
% colormap(CustomColormap);
% caxis([-10 10]);
% hold off;
end


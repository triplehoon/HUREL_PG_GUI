function [h] = plotShiftMergeMap(xgrid,ygrid, mu, rangeError, lim_caxis)
%UNTITLED4 이 함수의 요약 설명 위치
%   자세한 설명 위치

figure('Position', [200 200 600 500]);
hold on; box on;
% sz = zeros(size(mu,1),size(mu,2));
data = zeros(size(xgrid,2)*size(ygrid,2),4);
idx_pos=1;
% max_mu = max(max(mu));
ref_mu = 10;
for idx_x = 1:size(xgrid,2)
    for idx_y = 1:size(ygrid,2)
        if (mu(idx_x,idx_y))>0.001
            sz = mu(idx_x,idx_y)/ref_mu*100;
            data(idx_pos,:) = [-ygrid(idx_y) xgrid(idx_x), sz, rangeError(idx_x,idx_y)];
            idx_pos = idx_pos+1;
%             scatter(-ygrid(idx_y), xgrid(idx_x),10*mu(idx_x,idx_y), rangeError(idx_x,idx_y), 'filled');
        else
            sz = NaN;
            data(idx_pos,:) = [-ygrid(idx_y) xgrid(idx_x), sz, rangeError(idx_x,idx_y)];
            idx_pos = idx_pos+1;
        end
    end
end
h = scatter(data(:,1),data(:,2),data(:,3),data(:,4), 'filled');
% xlim([-70 70]); ylim([-70 70]);
ylim([min(ygrid)-10 max(ygrid)+10 ]);
xlim([min(xgrid)-10 max(xgrid)+10]);
load('colormap_jetEdited2.mat');
c = colorbar();
c.Label.String = 'Difference between planned and measured ranges (mm)';
c.Label.FontSize = 12;

colormap(colormap_jetEdited2);
caxis(lim_caxis);
% viscircles([25, 25],15,'Color','k', 'LineStyle','--', 'EnhanceVisibility', 0, 'LineWidth', 1);
% viscircles([25, -25],15,'Color','k', 'LineStyle','--', 'EnhanceVisibility', 0, 'LineWidth', 1);
% viscircles([-25, 25],15,'Color','k', 'LineStyle','--', 'EnhanceVisibility', 0, 'LineWidth', 1);
% viscircles([-25, -25],15,'Color','k', 'LineStyle','--', 'EnhanceVisibility',0, 'LineWidth', 1);

xlabel('x position (mm)');
ylabel('y position (mm)');
set(gca, 'FontSize', 12);
set(gca, 'FontWeight', 'bold');
end


function [data_pg, data_plan, data_log]= loadPGdata(pathfile_datalist, path_pg_matfile, path_pld2d, path_pld3d, path_log, caseNum)
tic;
datalist = readtable(pathfile_datalist);
idx = find(datalist.numCase == caseNum);
% path_pg_matfile = '..\data\data_sorted\pg_mat';
data_pg = load([path_pg_matfile '\' datalist.pgFileName{idx}]);
fieldname = fieldnames(data_pg);
eval(['data_pg = data_pg.', fieldname{1},';']);
fprintf(['PG data file ', datalist.pgFileName{idx}, ' loaded!\n'] );
if ~isempty(datalist.spot3DFileName{idx}) % both exists
    data_plan = readSpotData([path_pld2d, '\', datalist.pldFileName{idx},'.pld'], [path_pld3d, '\', datalist.spot3DFileName{idx},'.pld']);
    fprintf(['Plan pld file ', datalist.pldFileName{idx}, ' and 3D spot file ', datalist.spot3DFileName{idx},  ' loaded!\n'] );
elseif ~isempty(datalist.pldFileName{idx})
    data_plan = readSpotData_onlyPLD([path_pld2d, '\', datalist.pldFileName{idx},'.pld']);
else
    data_plan = [];
end
data_plan = data_plan(data_plan(:,6)~=0,:);
key_log = datalist.logFileName{idx};
data_log = readLogNCC3( path_log,  key_log);
fprintf(['Log data file ', datalist.logFileName{idx}, ' loaded!\n'] );
disp('Data loading finished');toc;
end
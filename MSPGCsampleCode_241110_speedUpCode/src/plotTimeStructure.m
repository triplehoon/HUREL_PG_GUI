function [] = plotTimeStructure(timeData,displayName,style)
%UNTITLED7 이 함수의 요약 설명 위치
%   자세한 설명 위치
onOffs = repmat([0;100;100;0],size(timeData,1),1);
times = zeros(size(timeData,1)*4,1);
times(1:4:end) = timeData(:,1);
times(2:4:end) = timeData(:,1);
times(3:4:end) = timeData(:,2);
times(4:4:end) = timeData(:,2);
plot(times,onOffs,style, 'DisplayName', displayName);
end

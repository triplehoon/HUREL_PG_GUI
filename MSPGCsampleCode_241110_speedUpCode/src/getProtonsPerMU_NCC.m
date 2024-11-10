function [protonsPerMU] = getProtonsPerMU_NCC(energy)
%UNTITLED 이 함수의 요약 설명 위치
%   자세한 설명 위치
data = zeros(6,2);
data(:,1) = [95.09;117.54;142.11;166.73;191.79;221.86]; %energy
data(:,2) = [8.73E+07;9.72E+07;1.06E+08;1.22E+08;1.49E+08;1.45E+08]; %protons per MU;
protonsPerMU = interp1(data(:,1), data(:,2), energy);
end
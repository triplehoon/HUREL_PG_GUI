classdef detector < handle
    % this class is for energy calibration of multi-slit camera
    %   자세한 설명 위치

    properties
        num
        adu

        spectCs
        snrCs
        noiseCs
        minSnrCs
        peakIdxsCs

        spectNa
        snrNa
        noiseNa
        minSnrNa
        peakIdxsNa

        idx511
        idx662
        idx1275
        idxThreshold

        peakSearchCs
        peakSearchNa

        fitobject
        gof
        isSuccess
    end

    methods
        function obj = detector(num, adu, spectCs,spectNa)
            obj.adu = adu;
            obj.num = num;
            obj.spectCs = spectCs;
            obj.spectNa = spectNa;

            obj.snrCs = [];
            obj.noiseCs  = [];
            obj.minSnrCs = [];
            obj.peakIdxsCs = [];
            obj.snrCs = [];
            obj.noiseCs = [];
            obj.minSnrCs = [];
            obj.peakIdxsCs = [];

            obj.idx511 = [];
            obj.idx662 = [];
            obj.idx1275 = [];
            obj.idxThreshold = [];
            obj.isSuccess = nan;
        end

        function obj = getCsData(obj, peakSearchData)
            obj.peakSearchCs = peakSearchData;
            obj.snrCs = peakSearchData.snr;
            obj.noiseCs = peakSearchData.noise;
            obj.minSnrCs = peakSearchData.min_snr;
            obj.peakIdxsCs = peakSearchData.peaks_idx;
            return
        end

        function obj = getNaData(obj, peakSearchData)
            obj.peakSearchNa = peakSearchData;
            obj.snrNa = peakSearchData.snr;
            obj.noiseNa = peakSearchData.noise;
            obj.minSnrNa = peakSearchData.min_snr;
            obj.peakIdxsNa = peakSearchData.peaks_idx;
            return
        end

        function obj = calibrate(obj, lim_p2, lim_r2)
            obj.idx662 = max(obj.peakIdxsCs);
            obj.idx1275 = max(obj.peakIdxsNa);
            obj.idx511 = max(obj.peakIdxsNa(obj.peakIdxsNa < obj.idx662));
            [obj.fitobject, obj.gof] = fit([511; 661.657; 1274.537], [obj.idx511; obj.idx662; obj.idx1275], 'poly1'); %정확한 에너지 반영하기
%             disp(['peak searched for det ' num2str(obj.num) ',  R2 = ' num2str(obj.gof.adjrsqared)])
            if abs(obj.fitobject.p2) < lim_p2 &&  obj.gof.adjrsquare > lim_r2
                obj.isSuccess = true;
            else
                obj.isSuccess = false;
                warning('Calibration for detector %d should be checked. adjusted R2 = %f, offset(p2) = %f',obj.num, obj.gof.adjrsquare, obj.fitobject.p2);
                obj.plotCalibResult;
            end
            return
        end

        function plotCalibResult(obj)
            figure('Position',[100 100 1800 600]);
            subplot(1,2,1);
            hold on; box on; grid on;
            yyaxis left
            p1 = plot(obj.adu, obj.peakSearchCs.spectrum.counts, 'b-', 'DisplayName','Cs spectrum');
            p2 =  plot(obj.adu, obj.peakSearchNa.spectrum.counts, 'r-', 'DisplayName','Na spectrum');
            xlim([min(obj.adu) max(obj.adu)]);
            xline(obj.idx511, 'g', '511 keV loc.')
            xline(obj.idx662, 'g', '662 keV loc.')
            xline(obj.idx1275, 'g', '1275 keV loc.')
            set(gca, 'YScale','log');

            yyaxis right
            p3 = plot(obj.adu+1, obj.snrCs, 'b.', 'DisplayName', 'SNR (Cs)');
            p4 = plot(obj.adu+1, obj.snrNa, 'r.', 'DisplayName', 'SNR (Na)');
            yline(obj.minSnrCs, 'b', 'min. SNR (Cs)')
            yline(obj.minSnrNa, 'r', 'min. SNR (Na)')
            ylim([0 max(obj.minSnrCs, obj.minSnrNa)*10])
            legend([p1, p2, p3, p4], 'Location','northeastoutside');
            title(['peak search for detector ' num2str(obj.num, '%03d')])
            hold off

            subplot(1,2,2);
            hold on; box on; grid on;
            scatter([511; 661.657; 1274.537], [obj.idx511; obj.idx662; obj.idx1275], 'DisplayName', 'fit data');
            %             plot(obj.fitobject, [511; 661.657; 1274.537], [obj.idx511; obj.idx662; obj.idx1275]);
            xlim([0 1500]);
            ylim([0 inf]);
            plot(obj.fitobject);

            ylabel('loc. (adu)');
            xlabel('energy (keV)');
            text(0.9,0.1,['fit line: y = ' num2str(obj.fitobject.p1) 'x + ' num2str(obj.fitobject.p2),', R2 = ' num2str(obj.gof.adjrsquare)], 'Units', 'normalized', 'HorizontalAlignment','right')

            title(['fit result for detector ' num2str(obj.num, '%03d')])
            legend('fit data', 'fit line', 'Location','northeastoutside');
            hold off;


        end

        function plotCsData(obj, xLim)
            figure; hold on; box on; grid on;
            plot(obj.adu, obj.spectCs, 'DisplayName','spectrum')
            yyaxis right
            plot(obj.adu, obj.snrCs, 'DisplayName', 'SNR')
            plot(obj.adu, obj.noiseCs, 'DisplayName', 'noise')
            yline(obj.minSnrCs, 'DisplayName', 'min. SNR')
            xline(obj.peakIdxsCs, 'DisplayName', 'searched peaks')
            xlim(xLim)
            title(['detector ' num2str(obj.num, '%03d'), ' Cs-137'])
            hold off
        end

        function plotNaData(obj, xLim)
            figure; hold on; box on; grid on;
            plot(obj.adu, obj.spectNa, 'DisplayName','spectrum')
            yyaxis right
            plot(obj.adu, obj.snrNa, 'DisplayName', 'SNR')
            plot(obj.adu, obj.noiseNa, 'DisplayName', 'noise')
            yline(obj.minSnrNa, 'DisplayName', 'min. SNR')
            xline(obj.peakIdxsNa, 'DisplayName', 'searched peaks')
            xlim(xLim)
            legend();
            title(['detector ' num2str(obj.num, '%03d'), ' Na-22'])
            hold off
        end


    end
end
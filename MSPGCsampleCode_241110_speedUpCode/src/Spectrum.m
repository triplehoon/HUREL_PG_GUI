classdef Spectrum < handle
    properties
        counts
        channels
        energies
        livetime
        e_units
        x
        x_units
        y_label
        plot_label
    end
    methods
        function self = Spectrum(counts, energies, channels, e_units, livetime)
            self.e_units = e_units;
            if isnan(counts)
                disp("ERROR: Must specify counts");
            end
            if isnan(channels)
                channels = 0:1:length(counts)-1;
            end
            if ~isnan(energies)
                self.energies = energies;
                self.x = energies;
                if isnan(e_units)
                    self.x_units = "Energy";
                else
                    self.x_units = "Energy2";
                end
            else
                self.energies = energies;
                self.x = channels;
                self.x_units = "Channels";
            end
            self.counts = counts;
            self.channels = channels;
            self.livetime = livetime;
            self.y_label = "Cts";
            self.plot_label = nan;
        end

        function smooth(self, num)
            self.counts = movmean(self.counts, num);
        end

        function rebin(self)
            cts = self.counts;
            if mod(length(cts), 2) ~= 0
                cts = cts(1:end-1);
            end
            y0 = cts(1:2:end);
            y1 = cts(2:2:end);
            y = y0 + y1;
            if isnan(self.energies)
                self.counts = y;
            else
                erg = self.energies;
                if mod(length(erg), 2) ~= 0
                    erg = erg(1:end-1);
                end
                e0 = erg(1:2:end);
                e1 = erg(2:2:end);
                en = (e0 + e1)/2;
                self.counts = y;
                self.energies = en;
            end
        end

        function plot(self)
            if isnan(self.plot_label)
                if isnan(self.livetime)
                    lt = "Livetime = N/A";
                else
                    lt = "Livetime = %3f";
                    lt = sprintf(lt, self.livetime);
                end
            end
            plot(self.x, self.counts);
            xlabel(self.x_units);
            ylabel(self.y_label);
            set(gca, 'YScale', 'log') 
        end
    end
end
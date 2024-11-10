
classdef PeakSearch < handle
    properties
        sepctrum
        ref_x
        ref_fwhm
        fwhm_at_0
        min_snr
        xrange
        channel_idx
        spectrum
        snr
        peak_plus_bkg
        bkg
        edg
        signal
        noise
        peaks_idx
        fwhm_guess
    end
    methods
        function self = PeakSearch(spectrum, ref_x, ref_fwhm, fwhm_at_0, min_snr, xrange)
            if class(spectrum) ~= "Spectrum"
                disp("'sepctrum' must be a Spectrum object");
            end
            if isnan(xrange)
                self.channel_idx = spectrum.channels;
                self.xrange = xrange;
            elseif length(xrange)==2 && ~isnan(spectrum.energies)
                ixe = (spectrum.energies >= xrange(1)) & (spectrum.energies <= xrange(2));
                erange = spectrum.channels(ixe);
                self.xrange = [erange(1), erange(end-1)];
            elseif length(xrange)==2 && isnan(spectrum.energies)
                self.xrange = xrange;
            else
                disp("Please check that the length of xrange is 2.")
            end
            self.ref_x = ref_x;
            self.ref_fwhm = ref_fwhm;
            self.fwhm_at_0 = fwhm_at_0;
            self.spectrum = spectrum;
            self.min_snr = min_snr;
            self.snr = [];
            self.peak_plus_bkg = [];
            self.bkg = [];
            self.signal = [];
            self.noise = [];
            self.peaks_idx = [];
            self.fwhm_guess = [];
            self.calculate();
        end

        % Calculate the expected FWHM at the given x value
        function fwhm_sqr = fwhm(self, x)
            f0 = self.fwhm_at_0;
            f1 = self.ref_fwhm;
            x1 = self.ref_x;
            %             fwhm_sqr = sqrt(f1 / sqrt(x1)) * sqrt(x) + f0;
            fwhm_sqr = sqrt(f0^2 + (f1^2 - f0^2) * (x / x1));
            return
        end

        % Generate the kernel for the given x value.
        function kern = kernel(self, x, edges)
            fwhm1 = self.fwhm(x);
            sigma = fwhm1 / 2.355;
            g1_x0 = gaussian_derivative(edges(1:end-1), x, sigma);
            g1_x1 = gaussian_derivative(edges(2:end), x, sigma);
            kern = g1_x0 - g1_x1;
            return
        end

        % Build a matrix of the kernel evaluated at each x value.
        function kmat = kernel_matrix(self, edges)
            n_channels = length(edges) - 1;
            kern = zeros(n_channels, n_channels);
            for i =  1:(length(edges)-1)
                kern(:, i) = self.kernel(edges(i), edges);
            end
            kern_pos = kern;
            kern_pos(kern_pos < 0) = 0;
            kern_neg = kern;
            kern_neg(kern_neg > 0) = 0;
            kern_neg = -1 * kern_neg;
            kern_neg = kern_neg.*(sum(kern_pos, 1)/sum(kern_neg, 1));
            kmat = kern_pos - kern_neg;
            return
        end

        % Convolve kernel with the data.
        function [peak_plus_bkg, bkg, signal, noise, snr] = convolve(self, edges, data)
            kern_mat = self.kernel_matrix(edges);
            kern_mat_pos = kern_mat;
            kern_mat_pos(kern_mat_pos < 0) = 0;
            kern_mat_neg = kern_mat;
            kern_mat_neg(kern_mat_neg > 0) = 0;
            kern_mat_neg = -1 * kern_mat_neg;
            data = data';
            peak_plus_bkg = kern_mat_pos * data;
            bkg = kern_mat_neg * data;
            signal = kern_mat * data;
            noise = (kern_mat.^2) * data;
            noise = sqrt(noise);
            snr = zeros(size(signal),'like',signal);
            snr(noise>0) = signal(noise>0)./noise(noise>0);
            return
        end

        % Calculate the convolution of the spectrum with the kernel.
        function calculate(self)
            if isnan(self.xrange)
                self.edg = [self.spectrum.channels self.spectrum.channels(end-1)+1];
                [ppb, b, sg, n, s] = self.convolve(self.edg, self.spectrum.counts);
            else
                x0 = self.xrange(1);
                x1 = self.xrange(2);
                self.channel_idx = (self.spectrum.channels >= x0) & (self.spectrum.channels <= x1);
                new_ch = self.spectrum.channels(self.channel_idx);
                new_cts = self.spectrum.counts(self.channel_idx);
                self.edg = [new_ch new_ch(end-1)+1];
                [ppb, b, sg, n, s] = self.convolve(self.edg, new_cts);
            end
            s(s<0) = 0;
            [~, pidx] = findpeaks(s, 'MinPeakProminence', self.min_snr);
            self.fwhm_guess = self.fwhm(pidx);
            self.peak_plus_bkg = ppb;
            self.bkg = b;
            self.signal = sg;
            self.noise = n;
            self.snr = s;
            if isnan(self.xrange)
                self.peaks_idx = pidx;
            else
                self.peaks_idx = new_ch(pidx);
            end
        end

        % Plot spectrum and their found peak positions.
        function plot_peaks(self)
            if isnan(self.spectrum.energies)
                x = self.spectrum.channels(self.channel_idx+1);
            else
                x = self.spectrum.energies(self.channel_idx+1);
            end
            hold on
            self.spectrum.plot();
            %plot(x, self.snr);
            for xc = 1:length(self.peaks_idx)
                idxnum = self.peaks_idx(xc);
                if isnan(self.spectrum.energies)
                    x0 = idxnum;
                else
                    x0 = self.spectrum.energies(idxnum);
                end
                xline(x0, "r-");
            end
            set(gca, 'YScale', 'log')
            ylim([0.1, 10000])
            hold off
        end
    end
end

function gauss = gaussian(x, mean, sigma)
z = (x-mean) / sigma;
gauss = exp((-z.^2)/2.0);
return
end

function gderiv = gaussian_derivative(x, mean, sigma)
z = x-mean;
gauss = gaussian(x, mean, sigma);
gderiv = -1*(z.*gauss);
return
end


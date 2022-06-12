function w = FFT(input_args)
% Fs = 1000000;
% nfft = size(input_args,2);
% cxn = xcorr(input_args,'unbiased');  %计算自相关函数
% CXk = fft(cxn,nfft);
% psd2 = abs(CXk);
% psd2 = (psd2-min(psd2))/(max(psd2)-min(psd2));
% w = psd2;
f = fft(input_args);
Pyy = f.*conj(f);
Pyy = (Pyy-min(Pyy))/(max(Pyy)-min(Pyy));
w = Pyy;
end
function [value,fre] = FindPeaks(data,mh,mpd)
[val,col] = findpeaks(data,"MinPeakHeight",mh,"MinPeakDistance",mpd);
value = val;
fre = col;
end
function [dataFinal] = loadPgBinFile(varargin)
% [file, path]= uigetfile('*.bin');
if nargin == 2
    path = varargin{1};
    file = varargin{2};
    fid = fopen(path+"\"+file);
elseif nargin ==1
        fid = fopen(varargin{1});
else
    error('PG bin file is not valid!');
end


mode = fgetl(fid);

if (~strcmp(mode, 'PMODE'))
    error("PMODE ONLY");
end
count = 0;
while (1)

    chk = fread(fid,1,'uint8');
    if (chk == 254)

        count = count + 1;
    else
        count = 0;
    end
    if (count == 4)
        chk = fread(fid,1,'uint8');
        if (chk ~=254)
            fseek(fid,ftell(fid)-5,'bof');
            break;
        else
            fseek(fid,ftell(fid)-1,'bof');
            count = 0;
        end
    end
end

chk = [254;254;254;254];
linecount = 1;
% tic;
bufferChunk =  50000000;

data = [];
count255 = 0;
while (1)
    if (feof(fid))
        break;
    end
    chunkData = uint8(fread (fid, [20, bufferChunk], 'uint8'));

    chunkidx = 0;
    i  = 5;
    chkChunk = typecast(reshape(chunkData(i-4:i-1,:), [], 1),'uint32')== 4278124286;

    SEC_TIME = typecast(reshape(chunkData(i:i+3,:), [], 1),'uint32');

    CH_NUMBER = typecast(reshape(chunkData(i+4:i+5,:), [], 1),'uint16');
    PRE_DATA = typecast(reshape(chunkData(i+6:i+7,:), [], 1),'uint16');
    V_PULSE_DATA = typecast(reshape(chunkData(i+10:i+11,:), [], 1),'uint16');
    T_PULSE_TIME = typecast(reshape(chunkData(i+12:i+15,:), [], 1),'uint32');
    

    if (size(chkChunk,1) ~= sum(chkChunk))
        for i = 1:size(chkChunk,1)
            if ~chkChunk(i)
                falseIdx = i;
                break;
            end
        end
        fseek(fid,ftell(fid)-20*(size(chkChunk,1) - falseIdx - 1),'bof');
        count255 = 0;
        count = 0;
        while (1)

            chk = fread(fid,1,'uint8');
            if (chk == 254)

                count = count + 1;
            elseif (chk == 255)
                count255 = count255 + 1;
            else
                count = 0;
                count255 = 0;
            end
            if (count == 4)
                chk = fread(fid,1,'uint8');
                if (chk ~=254)
                    fseek(fid,ftell(fid)-5,'bof');
                    break;
                else
                    fseek(fid,ftell(fid)-1,'bof');
                    count = 0;
                end
            end
            if (count255 == 10)
                fseek(fid,0,'eof');
                break;
            end
        end

    else
        falseIdx  = size(chkChunk,1);
    end


    datatmp = [double(CH_NUMBER(1:falseIdx-1)), double(SEC_TIME(1:falseIdx-1)), double(T_PULSE_TIME(1:falseIdx-1)),  double(V_PULSE_DATA(1:falseIdx-1))];



    data = [data ; datatmp];

    toc;

    if (count255 == 10)
        fseek(fid,0,'eof');
        break;
    end
end


% toc;
%%
timeInNano = double(data(:,2) * 1e10 + data(:,3) * 10) .* double(data(:,1) == 144) + double(data(:,2) * 1e10 + data(:,3) * 8) .* double(data(:,1) ~= 144);

dataFinal = [data(:,1),timeInNano ,data(:,4)];


% save(file(1:end-4) + ".mat",'data','-v7.3')
fclose(fid);

end
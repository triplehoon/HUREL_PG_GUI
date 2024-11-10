function [dataSet_agg, dataset_pg_agg] = agg_spotwise_3Dgauss_v5(gridFoVmm, dataSet, spotPGmm, sigma, direction)
            % 이 함수는 cylinder 형태의 spot aggregation을 수행합니다.

            if size(dataSet,1) ~= size(spotPGmm,1)
                error('data size not match (dataset_spot ~= dataset_pg)');
            end

            numSpot = size(dataSet,1);
            dataSet_agg = zeros(numSpot, size(dataSet,2) + 2);
            dataSet_agg(:, 1:end-2) = dataSet;
            col_diff = size(dataSet,2) + 1;
            col_aggmu = size(dataSet,2) + 2;
            dataset_pg_agg = zeros(size(spotPGmm,1), size(spotPGmm,2));

            for idx_query = 1:numSpot
                if dataSet_agg(idx_query, 14) == 0 % plan과 match되지 않은 스팟은 스킵합니다.
                    continue;
                end
                xpos_q = dataSet_agg(idx_query,11); % plan 상 스팟 위치
                ypos_q = dataSet_agg(idx_query,12);
                zpos_q = dataSet_agg(idx_query,13);
                for idx_temp = 1:numSpot
                    if dataSet_agg(idx_temp, 14) == 0 % plan과 match되지 않은 스팟은 스킵합니다.
                        continue;
                    end
                    xpos_t = dataSet_agg(idx_temp,11);
                    ypos_t = dataSet_agg(idx_temp,12);
                    zpos_t = dataSet_agg(idx_temp,13);

                    distance = sqrt((xpos_t - xpos_q)^2 + (ypos_t - ypos_q)^2 + (zpos_t - zpos_q)^2);
                    if distance > 3 * sigma
                        continue;
                    end
                    if sigma == 0
                        weight = 1;
                    else
                        weight = exp(-0.5 * (distance / sigma)^2);
                    end

                    energy = dataSet_agg(idx_temp,10);
                    if direction
                        xgrid_temp = gridFoVmm - dataSet_agg(idx_temp,13);
                    else
                        xgrid_temp = gridFoVmm + dataSet_agg(idx_temp,13);
                    end

                    % 보간 수행
                    dist_shift = qinterp1(xgrid_temp, spotPGmm(idx_temp,:), gridFoVmm)';

                    % NaN 값을 최근접 값으로 대체 (fillmissing 대체)
                    nanIdx = isnan(dist_shift);
                    if any(nanIdx)
                        validIdx  = ~nanIdx;
                        % 앞쪽 NaN 처리
                        firstValid = find(validIdx, 1, 'first');
                        dist_shift(1:firstValid-1) = dist_shift(firstValid);
                        % 중간 NaN 처리
                        for k = firstValid:length(dist_shift)
                            if isnan(dist_shift(k))
                                dist_shift(k) = dist_shift(k-1);
                            end
                        end
                    end

                    dataset_pg_agg(idx_query,:) = dataset_pg_agg(idx_query,:) + dist_shift * weight;
                    dataSet_agg(idx_query, 16) = dataSet_agg(idx_query, 16) + dataSet_agg(idx_temp, 14);
                end
                if sum(dataset_pg_agg(idx_query, :)) == 0
                    dataSet_agg(idx_query,15) = NaN;
                else
                    dataSet_agg(idx_query,15) = GetRange_ver4p3(gridFoVmm, dataset_pg_agg(idx_query, :), 0, direction);
                end
            end
            fprintf('Merging finished \n');
        end

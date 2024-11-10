function index = bns(list, listSearchIndex, value, startIndex)
% Binary Search Function in MATLAB (modified to find closest value)
% Assumes the input list is sorted
% Inputs:
% - list: A sorted list of numbers
% - value: The value to search for in the list
% Output:
% - index: The index of the closest value in the list

low = startIndex;
high = length(list);
closest_index = low;  % Start by assuming the closest is the first element

% Initialize the closest difference with the value at the low index
closest_diff = abs(list(low,listSearchIndex) - value);


while low <= high
    mid = floor((low + high) / 2);  % Find the middle index

    % Check if the current mid is closer to the value
    current_diff = abs(list(mid,listSearchIndex) - value);
    if current_diff < closest_diff
        closest_diff = current_diff;
        closest_index = mid;
    end

    % Compare target value with mid value
    if list(mid,listSearchIndex) < value
        low = mid + 1;  % Search in the right half
    else
        high = mid - 1;  % Search in the left half
    end
end

% After the loop, low and high will be just outside the search range.
% We should check both to ensure closest value:
if low <= length(list)  % Check if low is still within bounds
    low_diff = abs(list(low,listSearchIndex) - value);
    if low_diff < closest_diff
        closest_index = low;
    end
end

if high >= 1  % Check if high is still within bounds
    high_diff = abs(list(high,listSearchIndex) - value);
    if high_diff < closest_diff
        closest_index = high;
    end
end

index = closest_index;  % Return the index of the closest value
end
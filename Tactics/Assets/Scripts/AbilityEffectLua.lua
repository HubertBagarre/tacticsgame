-- using stringToConditionalIndex
-- using stringToConditionIndex
-- using stringToEffectIndex
-- using canCastConditionalTable
-- using canCastTable

function ConvertToConditionalIndex(value)
    converted = stringToConditionalIndex[value]
    if converted ~= nil then
        return converted
    end
    return value
end

function CanCastCondtionalEffect(value)
    index = ConvertToConditionalIndex(value)
    return canCastConditionalTable[index]
end

function AddEffect(value)
    index = ConvertToConditionalIndex(value)
    output[index] = true
end

function RemoveEffect(value)
    index = ConvertToConditionalIndex(value)
    output[index] = false
end

function Log(text)
    output[tostring(text)] = true
end

function Main()
    %PARAMETERS%
end

output = {}

Main()

return output


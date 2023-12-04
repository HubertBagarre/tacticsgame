--check condition
function MatchesCondition(value)
    return injector.CanCast(value)
end

--add effect
function AddEffect(value)
     injector.AddEffect(value)
end

--try add effect
function TryAddEffect(value)
    return injector.TryAddEffect(value)
end

function Log(message)
    injector.Log(tostring(message))
end

function Main()
    %PARAMETERS%
end

Main()

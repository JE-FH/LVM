T = {
    TokenPattern = {}
}
T.TokenPattern.__index = T.TokenPattern;

function T.R(from, to)
    assert(type(from) == "string" and #from == 1, "from must be a single character")
    assert(type(to) == "string" and #to == 1, "from must be a single character")
    local iFrom = string.byte(from);
    local iTo = string.byte(to);
    assert(iFrom <= iTo, "from must be less than or equal to to")
    local literals = {}
    for i = iFrom, iTo do
        table.insert(literals, string.char(i))
    end
    return T.L(literals);
end

function T.L(literalOrLiterals)
    assert(type(literalOrLiterals) == "string" or type(literalOrLiterals) == "table", "literalOrLiterals must be a string or table of strings");
    if (type(literalOrLiterals) == "string") then
        return T.TokenPattern:new({ literalOrLiterals }, {});
    else
        return T.TokenPattern:new(literalOrLiterals, {});
    end
end

function T.P(patternOrPatterns)
    assert(type(patternOrPatterns) == "string" or type(patternOrPatterns) == "table", "patternOrPatterns must be a string or table of strings");
    if (type(patternOrPatterns) == "string") then
        return T.TokenPattern:new({}, { patternOrPatterns });
    else
        return T.TokenPattern:new({}, patternOrPatterns);
    end
end

function T.TokenPattern:new(literals, patterns)
    assert(type(literals) == "table", "literals must be a table of strings")
    assert(type(patterns) == "table", "patterns must be a table of strings")
    local o = {
        literals = literals,
        patterns = patterns
    };
    setmetatable(o, self);
    return o;
end

function T.TokenPattern:__add(rhs)
    assert(getmetatable(rhs) == T.TokenPattern, "rhs must be a TokenPattern")
    for k, v in ipairs(rhs.literals) do
        table.insert(self.literals, v)
    end
    for k, v in ipairs(rhs.patterns) do
        table.insert(self.patterns, v)
    end
    return self;
end

local G = {
    Rule = {}
}

G.Rule.__index = G.Rule;

function G.R(tokenOrNil)
    assert(tokenOrNil == nil or getmetatable(tokenOrNil) == T.TokenPattern, "tokenOrNil must be a TokenPattern or nil")
    local o = {
        variants = {}
    };
    setmetatable(o, G.Rule);
    if (tokenOrNil ~= nil) then
        table.insert(o.variants, tokenOrNil);
    end
    return o;
end

function G.Rule:__add(rhs)
    assert(getmetatable(rhs) == G.Rule, "rhs must be a Rule");
    for k, v in pairs(rhs.variants) do
        table.insert(self.variants, v);
    end
    return self;
end

function G.Rule:__mul(rhs)
    assert(getmetatable(rhs == g.Rule, "rhs must be a Rule"));
    local oldVariants = self.variants;
    self.variants = {};
    for _, rhsv in pairs(rhs.variants) do
        for _, lhsv in pairs(lhs.variants) do
            local newVariant = {};
            for _, v in pairs(lhsv) do
                table.insert(newVariant, v);
            end
            for _, v in pairs(rhsv) do
                table.insert(newVariant, v);
            end
            table.insert(self.variants, newVariant)
        end
    end
end

function dump(o)
    if type(o) == 'table' then
       local s = '{ '
       for k,v in pairs(o) do
          if type(k) ~= 'number' then k = '"'..k..'"' end
          s = s .. '['..k..'] = ' .. dump(v) .. ','
       end
       return s .. '} '
    else
        print(o)
       return tostring(o)
    end
 end
 

local Numerals = T.P("%d+");

local WS = T.L(" ") + T.L("\t") + T.L("\r") + T.L("\n");
local Op = T.L("+") + T.L("-") + T.L("*") + T.L("/");

local exp = G.R()
exp = exp + G.R(Numeral) + (exp * G.R(Op) * exp);

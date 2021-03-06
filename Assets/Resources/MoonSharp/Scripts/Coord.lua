Coord = {}
Coord.__index = Coord

local function newCoord( lon, lat, alt )
    return setmetatable( { lon = lon or 0, lat = lat or 0 , alt = alt or 0}, Coord )
end

function iscoord( cTbl )
    return getmetatable( cTbl ) == Coord
end

function Coord.__eq( a, b )
    return a.lon == b.lon and a.lat == b.lat and a.alt == b.alt
end

function Coord:__tostring()
    return string.format("(lon: %.2f°, lat: %.2f°, alt: %.2fm)", self.lon, self.lat, self.alt)
end

return setmetatable( Coord, { __call = function( _, ... ) return newCoord( ... ) end } )
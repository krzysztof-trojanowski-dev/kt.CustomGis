-- osm2pgsql flex style: poi, roads, buildings, places (SRID 4326)
local srid = 4326

local tables = {}

tables.poi = osm2pgsql.define_table({
    name = 'poi',
    ids = { type = 'any', id_column = 'osm_id', type_column = 'osm_type' },
    columns = {
        { column = 'name', type = 'text' },
        { column = 'category', type = 'text', not_null = true },
        { column = 'subtype', type = 'text', not_null = true },
        { column = 'tags', type = 'jsonb' },
        { column = 'geom', type = 'point', projection = srid, not_null = true },
    }
})

tables.roads = osm2pgsql.define_table({
    name = 'roads',
    ids = { type = 'way', id_column = 'osm_id' },
    columns = {
        { column = 'name', type = 'text' },
        { column = 'highway', type = 'text', not_null = true },
        { column = 'ref', type = 'text' },
        { column = 'maxspeed', type = 'text' },
        { column = 'tags', type = 'jsonb' },
        { column = 'geom', type = 'linestring', projection = srid, not_null = true },
    }
})

tables.buildings = osm2pgsql.define_table({
    name = 'buildings',
    ids = { type = 'any', id_column = 'osm_id', type_column = 'osm_type' },
    columns = {
        { column = 'name', type = 'text' },
        { column = 'building', type = 'text', not_null = true },
        { column = 'levels', type = 'text' },
        { column = 'tags', type = 'jsonb' },
        { column = 'geom', type = 'multipolygon', projection = srid, not_null = true },
    }
})

tables.places = osm2pgsql.define_table({
    name = 'places',
    ids = { type = 'node', id_column = 'osm_id' },
    columns = {
        { column = 'name', type = 'text' },
        { column = 'place', type = 'text', not_null = true },
        { column = 'population', type = 'text' },
        { column = 'tags', type = 'jsonb' },
        { column = 'geom', type = 'point', projection = srid, not_null = true },
    }
})

local poi_keys = { 'amenity', 'shop', 'tourism', 'leisure', 'historic', 'office', 'craft' }

local function poi_category(tags)
    for _, k in ipairs(poi_keys) do
        if tags[k] then return k, tags[k] end
    end
end

function osm2pgsql.process_node(object)
    local t = object.tags
    if t.place then
        tables.places:insert({
            name = t.name, place = t.place, population = t.population,
            tags = t, geom = object:as_point()
        })
    end
    local cat, sub = poi_category(t)
    if cat then
        tables.poi:insert({
            name = t.name, category = cat, subtype = sub,
            tags = t, geom = object:as_point()
        })
    end
end

function osm2pgsql.process_way(object)
    local t = object.tags
    if t.highway then
        tables.roads:insert({
            name = t.name, highway = t.highway, ref = t.ref, maxspeed = t.maxspeed,
            tags = t, geom = object:as_linestring()
        })
    end
    if t.building then
        tables.buildings:insert({
            name = t.name, building = t.building, levels = t['building:levels'],
            tags = t, geom = object:as_multipolygon()
        })
    end
    local cat, sub = poi_category(t)
    if cat and object.is_closed then
        tables.poi:insert({
            name = t.name, category = cat, subtype = sub,
            tags = t, geom = object:as_polygon():centroid()
        })
    end
end

function osm2pgsql.process_relation(object)
    local t = object.tags
    if t.type == 'multipolygon' and t.building then
        tables.buildings:insert({
            name = t.name, building = t.building, levels = t['building:levels'],
            tags = t, geom = object:as_multipolygon()
        })
    end
end

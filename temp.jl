module TwigHelperCrystalLazer

using ..Ahorn, Maple

@mapdef Entity "TwigHelper/CrystalLazerUp" CrystalLazerUp(x::Integer, y::Integer, 0, delay::Number=0.5, duration::Number=0.5, length::Integer=16, flag::String="sample_flag")
@mapdef Entity "TwigHelper/CrystalLazerDown" CrystalLazerDown(x::Integer, y::Integer, 1, delay::Number=0.5, duration::Number=0.5, length::Integer=16, flag::String="sample_flag")
@mapdef Entity "TwigHelper/CrystalLazerRight" CrystalLazerRight(x::Integer, y::Integer, 2,delay::Number=0.5, duration::Number=0.5, length::Integer=16, flag::String="sample_flag")
@mapdef Entity "TwigHelper/CrystalLazerLeft" CrystalLazerLeft(x::Integer, y::Integer, 3,delay::Number=0.5, duration::Number=0.5, length::Integer=16, flag::String="sample_flag")

const placements = Ahorn.PlacementDict(
    "Crystal Lazer (Up, Twig Helper)" => Ahorn.EntityPlacement(
        CrystalLazerUp
    ),
    "Crystal Lazer (Down, Twig Helper)" => Ahorn.EntityPlacement(
        CrystalLazerDown
    ),
    "Crystal Lazer (Right, Twig Helper)" => Ahorn.EntityPlacement(
        CrystalLazerRight
    ),
    "Crystal Lazer (Left, Twig Helper)" => Ahorn.EntityPlacement(
        CrystalLazerLeft
    ),
)

function Ahorn.selection(entity::CrystalLazerUp)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 3, 12, 5)
end

function Ahorn.selection(entity::CrystalLazerDown)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y, 12, 5)
end

function Ahorn.selection(entity::CrystalLazerLeft)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 1, y - 6, 5, 12)
end

function Ahorn.selection(entity::CrystalLazerRight)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 4, y - 6, 5, 12)
end

sprite = "danger/CrystalLazer/crystal_up_00.png"

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalLazerUp, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -8)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalLazerDown, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 16, 24, rot=pi)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalLazerLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 24, 0, rot=pi / 2)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalLazerRight, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, -8, 16, rot=-pi / 2)

end

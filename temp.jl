module TwigHelperInkrail

using ..Ahorn, Maple

@mapdef Entity "TwigHelper/Inkrail" Inkrail(x::Integer, y::Integer, launchSpeed::Number=240.0, decayRate::Number=1.0, xSineAmplitude::Number=0.0, xSineFrequency::Number=0.0, ySineAmplitude::Number=0.0, ySineFrequency::Number=0.0, overrideDashes::Bool=false, dashes::Integer=1, canJumpFromBooster::Bool=false, tint::String="ffffff")


const placements = Ahorn.PlacementDict(
   "Inkrail (WIP, Cursed) (Twig Helper)" => Ahorn.EntityPlacement(
	  Inkrail,
	  "point"
   )
)

sprite = "objects/boosterBase/boosterBase00"

function Ahorn.selection(entity::Inkrail)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Inkrail, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end

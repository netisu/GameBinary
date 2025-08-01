shader_type spatial;
render_mode depth_draw_always, blend_mix;

void fragment()
{
    ALBEDO = COLOR.rgb;
	ALPHA =  COLOR.a;
}
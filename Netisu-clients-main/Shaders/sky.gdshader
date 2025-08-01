shader_type sky;
render_mode use_quarter_res_pass;

group_uniforms sky;
	uniform vec3 day_top_color : source_color = vec3(0.1, 0.6, 1.0);
	uniform vec3 day_bottom_color : source_color = vec3(0.4, 0.8, 1.0);
	uniform vec3 sunset_top_color : source_color = vec3(0.7, 0.75, 1.0);
	uniform vec3 sunset_bottom_color : source_color = vec3(1.0, 0.5, 0.7);
	uniform vec3 night_top_color : source_color = vec3(0.02, 0.0, 0.04);
	uniform vec3 night_bottom_color : source_color = vec3(0.1, 0.0, 0.2);

group_uniforms horizon;
	uniform vec3 horizon_color : source_color = vec3(0.0, 0.7, 0.8);
	uniform float horizon_blur : hint_range(0.0, 1.0, 0.01) = 0.05;

group_uniforms sun;
	uniform vec3 sun_color : source_color = vec3(10.0, 8.0, 1.0);
	uniform vec3 sun_sunset_color : source_color = vec3(10.0, 0.0, 0.0);
	uniform float sun_size : hint_range(0.01, 1.0) = 0.2;
	uniform float sun_blur : hint_range(0.01, 20.0) = 10.0;

group_uniforms moon;
	uniform vec3 moon_color : source_color = vec3(1.0, 0.95, 0.7);
	uniform float moon_size : hint_range(0.01, 1.0) = 0.06;
	uniform float moon_blur : hint_range(0.01, 10.0) = 0.1;

group_uniforms clouds;
	uniform vec3 clouds_top_color : source_color = vec3(1.0, 1.0, 1.00);
	uniform vec3 clouds_middle_color : source_color = vec3(0.92, 0.92, 0.98);
	uniform float clouds_speed : hint_range(0.0, 20.0, 0.01) = 2.0;
	uniform float clouds_direction : hint_range(-0.5, 0.5, 0.0) = 0.2;
	uniform float clouds_scale : hint_range(0.0, 4.0, 0.01) = 1.0;
	uniform float clouds_cutoff : hint_range(0.0, 1.0, 0.01) = 0.3;
	uniform float clouds_fuzziness : hint_range(0.0, 2.0, 0.01) = 0.5;
	uniform float clouds_weight : hint_range(0.0, 1.0, 0.01) = 0.0;
	uniform float clouds_blur : hint_range(0.0, 1.0, 0.01) = 0.25;

group_uniforms stars;
	uniform sampler2D stars_texture : filter_linear_mipmap, hint_default_black;
	uniform float stars_speed : hint_range(0.0, 20.0, 0.01) = 1.0;
	uniform float stars_scale : hint_range(0.1, 3.0, 0.01) = 1.0;
	uniform float stars_opacity : hint_range(0.1, 1.0, 0.01) = 1.0;

group_uniforms settings;
	uniform float overwritten_time = 0.0;
	// Added uniforms to receive world-space light directions from C#
	uniform vec3 sun_dir_world = vec3(0.0, 1.0, 0.0);
	uniform vec3 moon_dir_world = vec3(0.0, -1.0, 0.0);

// OPTIMIZED NOISE FUNCTION
vec2 hash(vec2 p) {
	p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
	return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}

float simple_noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(dot(hash(i + vec2(0.0, 0.0)), f - vec2(0.0, 0.0)), 
					 dot(hash(i + vec2(1.0, 0.0)), f - vec2(1.0, 0.0)), u.x),
			   mix(dot(hash(i + vec2(0.0, 1.0)), f - vec2(0.0, 1.0)), 
					 dot(hash(i + vec2(1.0, 1.0)), f - vec2(1.0, 1.0)), u.x), u.y);
}

// Function needed to calculate the phase of the moon
float sphere_intersect(vec3 view_dir, vec3 sphere_pos, float radius) {
	float b = dot(-sphere_pos, view_dir);
	float c = dot(-sphere_pos, -sphere_pos) - pow(radius, 2.0);
	float h = pow(b, 2.0) - c;
	return h < 0.0 ? -1.0 : -b - sqrt(h);
}

void sky() {
	float time = overwritten_time != 0.0 ? overwritten_time : TIME;

	// --- SKY ---
	// Use the sun_dir_world uniform for all day/night calculations.
	float sunset_amount = clamp(0.5 - abs(sun_dir_world.y), 0.0, 0.5) * 2.0;
	float night_amount = clamp(-sun_dir_world.y + 0.7, 0.0, 1.0);

	float eyedir_y = smoothstep(0.0, 1.0, EYEDIR.y);
	vec3 day_sky = mix(day_bottom_color, day_top_color, eyedir_y);
	vec3 sunset_sky = mix(sunset_bottom_color, sunset_top_color, eyedir_y);
	vec3 night_sky = mix(night_bottom_color, night_top_color, eyedir_y);

	vec3 sky_color = mix(day_sky, sunset_sky, sunset_amount);
	sky_color = mix(sky_color, night_sky, night_amount);
	sky_color = mix(sky_color, vec3(0.0), clamp((0.7 - clouds_cutoff) * clouds_weight, 0.0, 1.0));

	COLOR = sky_color;

	// --- HORIZON ---
	float horizon_amount = 0.0;
	if (EYEDIR.y < 0.0) {
		horizon_amount = clamp(abs(EYEDIR.y) / horizon_blur, 0.0, 1.0);
		vec3 final_horizon_color = mix(horizon_color, sky_color, night_amount * 0.9);
		final_horizon_color = mix(final_horizon_color, vec3(0.0), (1.0 - clouds_cutoff) * clouds_weight * 0.7);
		COLOR = mix(COLOR, final_horizon_color, horizon_amount);
	}

	// --- MOON ---
	float moon_amount = 0.0;
	if (LIGHT1_ENABLED) {
		float m_size = moon_size + cos(moon_dir_world.y * PI) * moon_size * 0.25;
		float m_dist = distance(EYEDIR, LIGHT1_DIRECTION) / m_size;
		moon_amount = clamp((1.0 - m_dist) / moon_blur, 0.0, 1.0);
		if (moon_amount > 0.0) {
			float moon_intersect = sphere_intersect(EYEDIR, LIGHT1_DIRECTION, m_size);
			vec3 moon_normal = normalize(LIGHT1_DIRECTION - EYEDIR * moon_intersect);
			float moon_n_dot_l = pow(clamp(dot(moon_normal, -LIGHT0_DIRECTION), 0.05, 1.0), 2.0);
			moon_amount *= 1.0 - horizon_amount;
			COLOR = mix(COLOR, moon_color, moon_n_dot_l * moon_amount);
		}
	}

	// --- SUN ---
	float sun_dist = 0.0;
	if (LIGHT0_ENABLED) {
		sun_dist = distance(EYEDIR, LIGHT0_DIRECTION);
		float s_size = sun_size + cos(sun_dir_world.y * PI) * sun_size * 0.25;
		float sun_amount = clamp((1.0 - sun_dist / s_size) / sun_blur, 0.0, 1.0);
		if (sun_amount > 0.0) {
			float s_sunset_amount = (sun_dir_world.y > 0.0) ? clamp(cos(sun_dir_world.y * PI), 0.0, 1.0) : 1.0;
			vec3 final_sun_color = mix(sun_color, sun_sunset_color, s_sunset_amount);
			sun_amount = clamp(sun_amount * (1.0 - moon_amount), 0.0, 1.0);
			sun_amount *= 1.0 - horizon_amount;
			if (final_sun_color.r > 1.0 || final_sun_color.g > 1.0 || final_sun_color.b > 1.0) {
				final_sun_color *= sun_amount;
			}
			COLOR = mix(COLOR, final_sun_color, sun_amount);
		}
	}

	// --- STARS ---
	vec2 sky_uv = EYEDIR.xz / sqrt(max(EYEDIR.y, 0.001));
	if (EYEDIR.y > -0.01 && sun_dir_world.y < 0.0) {
		float stars_cos = cos(stars_speed * time * 0.005);
		float stars_sin = sin(stars_speed * time * 0.005);
		vec2 stars_uv = mat2(vec2(stars_cos, -stars_sin), vec2(stars_sin, stars_cos)) * sky_uv * stars_scale;
		vec3 stars_col = texture(stars_texture, stars_uv).rgb * -sun_dir_world.y;
		stars_col *= (1.0 - moon_amount);
		COLOR += stars_col * stars_opacity;
	}

	// --- OPTIMIZED CLOUDS ---
	if (EYEDIR.y > 0.0) {
		float c_speed = time * clouds_speed * 0.01;
		float sin_x = sin(clouds_direction * PI * 2.0);
		float cos_y = cos(clouds_direction * PI * 2.0);
		
		vec2 move1 = vec2(sin_x, cos_y) * c_speed;
		vec2 move2 = vec2(sin_x * 0.9, cos_y * 1.1) * c_speed * 0.8;
		
		float noise1 = simple_noise((sky_uv + move1) * clouds_scale * 0.7);
		float noise2 = simple_noise((sky_uv + move2) * clouds_scale * 1.5);
		
		float combined_noise = (noise1 + noise2) * 0.5;

		float cloud_map = smoothstep(clouds_cutoff, clouds_fuzziness, combined_noise);
		float clouds_amount = cloud_map * clamp(abs(EYEDIR.y) / clouds_blur, 0.0, 1.0);

		vec3 clouds_color = mix(clouds_middle_color, clouds_top_color, noise1);
		
		clouds_color = mix(clouds_color, clamp(sun_color, 0.0, 1.0), pow(1.0 - clamp(sun_dist, 0.0, 1.0), 5.0));
		clouds_color = mix(clouds_color, sunset_bottom_color, sunset_amount * 0.75);
		clouds_color = mix(clouds_color, sky_color, clamp(night_amount, 0.0, 0.98));
		clouds_color = mix(clouds_color, vec3(0.0), clouds_weight * 0.9);

		COLOR = mix(COLOR, clouds_color, clouds_amount);
	}
}

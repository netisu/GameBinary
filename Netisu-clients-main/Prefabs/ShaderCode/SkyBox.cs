using Godot;
using System;

[Tool]
public partial class SkyBox : Node3D
{
    private const float HoursInDay = 24.0f;
    private const int DaysInYear = 365;

    // This property allows manual control over the time of day.
    [Export]
    public bool ManualTimeControl { get; set; } = false;

    private float _dayTime = 12.0f;
    private float _latitude = 0.0f;
    private int _dayOfYear = 1;
    private float _planetAxialTilt = 23.44f;
    private float _moonOrbitalInclination = 5.14f;
    private float _moonOrbitalPeriod = 29.5f;
    private float _cloudsCutoff = 0.3f;
    private float _cloudsWeight = 0.0f;
    private bool _useDayTimeForShader = false;
    private float _timeScale = 0.01f;
    private float _sunBaseEnergy = 0.0f;
    private float _moonBaseEnergy = 0.0f;
     
    // Private fields to hold the world direction vectors
    private Vector3 _sunDirWorld;
    private Vector3 _moonDirWorld;

    [Export(PropertyHint.Range, "0.0,24.0,0.0001")]
    public float DayTime
    {
        get => _dayTime;
        set
        {
            _dayTime = value;
            bool dayChanged = false;
            
            while (_dayTime < 0.0f)
            {
                _dayTime += HoursInDay;
                DayOfYear -= 1;
                dayChanged = true;
            }
            while (_dayTime >= HoursInDay)
            {
                _dayTime -= HoursInDay;
                DayOfYear += 1;
                dayChanged = true;
            }

            // If the day didn't change, we must manually call UpdateAll().
            // If it did change, the DayOfYear setter will handle the update.
            if (!dayChanged)
            {
                UpdateAll();
            }
        }
    }
    public void SetDayTimeFromUI(float time)
    {
        ManualTimeControl = true;
        DayTime = time;
    }
    
    [Export(PropertyHint.Range, "-90.0,90.0,0.01")]
    public float Latitude
    {
        get => _latitude;
        set { _latitude = value; UpdateAll(); }
    }

    [Export(PropertyHint.Range, "1,365,1")]
    public int DayOfYear
    {
        get => _dayOfYear;
        set { _dayOfYear = value; UpdateAll(); }
    }

    [Export(PropertyHint.Range, "-180.0,180.0,0.01")]
    public float PlanetAxialTilt
    {
        get => _planetAxialTilt;
        set { _planetAxialTilt = value; UpdateAll(); }
    }

    [Export(PropertyHint.Range, "-180.0,180.0,0.01")]
    public float MoonOrbitalInclination
    {
        get => _moonOrbitalInclination;
        set { _moonOrbitalInclination = value; UpdateMoon(); }
    }

    [Export(PropertyHint.Range, "0.1,365,0.01")]
    public float MoonOrbitalPeriod
    {
        get => _moonOrbitalPeriod;
        set { _moonOrbitalPeriod = value; UpdateMoon(); }
    }

    [Export(PropertyHint.Range, "0.0,1.0,0.01")]
    public float CloudsCutoff
    {
        get => _cloudsCutoff;
        set { _cloudsCutoff = value; UpdateClouds(); }
    }

    [Export(PropertyHint.Range, "0.0,1.0,0.01")]
    public float CloudsWeight
    {
        get => _cloudsWeight;
        set { _cloudsWeight = value; UpdateClouds(); }
    }

    [Export]
    public bool UseDayTimeForShader
    {
        get => _useDayTimeForShader;
        set { _useDayTimeForShader = value; UpdateShader(); }
    }

    [Export(PropertyHint.Range, "0.0,1.0,0.0001")]
    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = value;
    }

    [Export(PropertyHint.Range, "0.0,10.0,0.01")]
    public float SunBaseEnergy
    {
        get => _sunBaseEnergy;
        set { _sunBaseEnergy = value; UpdateShader(); }
    }

    [Export(PropertyHint.Range, "0.0,10.0,0.01")]
    public float MoonBaseEnergy
    {
        get => _moonBaseEnergy;
        set { _moonBaseEnergy = value; UpdateShader(); }
    }

    [Export] public WorldEnvironment Environment { get; set; }
    [Export] public DirectionalLight3D Sun { get; set; }
    [Export] public DirectionalLight3D Moon { get; set; }

    public override void _Ready()
    {
        if (IsInstanceValid(Sun))
        {
            Sun.Position = Vector3.Zero;
            Sun.Rotation = Vector3.Zero;
            Sun.RotationOrder = EulerOrder.Zxy;
            if (_sunBaseEnergy == 0.0f)
            {
                _sunBaseEnergy = Sun.LightEnergy;
            }
        }
        if (IsInstanceValid(Moon))
        {
            Moon.Position = Vector3.Zero;
            Moon.Rotation = Vector3.Zero;
            Moon.RotationOrder = EulerOrder.Zxy;
            if (_moonBaseEnergy == 0.0f)
            {
                _moonBaseEnergy = Moon.LightEnergy;
            }
        }
        UpdateAll();
    }

    public override void _Process(double delta)
    {
        // Only advance time if not manually controlled.
        if (!ManualTimeControl && !Engine.IsEditorHint())
        {
            DayTime += (float)delta * _timeScale;
        }
    }

    private void UpdateAll()
    {
        UpdateSun();
        UpdateMoon();
        UpdateClouds();
        UpdateShader();
    }

    private void UpdateSun()
    {
        if (!IsInstanceValid(Sun)) return;
        float dayProgress = _dayTime / HoursInDay;
        float newRotationX = (dayProgress * 2.0f - 0.5f) * -(float)Math.PI;
        float earthOrbitProgress = (_dayOfYear + 193.0f + dayProgress) / DaysInYear;
        float newRotationY = Mathf.DegToRad(Mathf.Cos(earthOrbitProgress * Mathf.Pi * 2.0f) * _planetAxialTilt);
        float newRotationZ = Mathf.DegToRad(_latitude);
        Sun.Rotation = new Vector3(newRotationX, newRotationY, newRotationZ);
        
        _sunDirWorld = -Sun.GlobalTransform.Basis.Z.Normalized();
        
        Sun.LightEnergy = Mathf.SmoothStep(-0.05f, 0.1f, _sunDirWorld.Y) * _sunBaseEnergy;
    }


    private void UpdateMoon()
    {
        if (!IsInstanceValid(Moon)) return;
        float dayProgress = _dayTime / HoursInDay;
        float moonOrbitProgress = ((float)_dayOfYear % _moonOrbitalPeriod + dayProgress) / _moonOrbitalPeriod;
        float newRotationX = ((dayProgress - moonOrbitProgress) * 2.0f - 1.0f) * (float)Math.PI;
        float axialTilt = _moonOrbitalInclination + (_planetAxialTilt * Mathf.Sin((dayProgress * 2.0f - 1.0f) * (float)Math.PI));
        float newRotationY = Mathf.DegToRad(axialTilt);
        float newRotationZ = Mathf.DegToRad(_latitude);
        Moon.Rotation = new Vector3(newRotationX, newRotationY, newRotationZ);
        
        _moonDirWorld = -Moon.GlobalTransform.Basis.Z.Normalized();
        
        Moon.LightEnergy = Mathf.SmoothStep(-0.05f, 0.1f, _moonDirWorld.Y) * _moonBaseEnergy;
    }

    private void UpdateClouds()
    {
        if (IsInstanceValid(Environment) && Environment.Environment?.Sky?.SkyMaterial is ShaderMaterial skyMaterial)
        {
            skyMaterial.SetShaderParameter("clouds_cutoff", _cloudsCutoff);
            skyMaterial.SetShaderParameter("clouds_weight", _cloudsWeight);
        }
    }

    private void UpdateShader()
    {
        if (IsInstanceValid(Environment) && Environment.Environment?.Sky?.SkyMaterial is ShaderMaterial skyMaterial)
        {
            float overwrittenTime = _useDayTimeForShader ? (_dayOfYear * HoursInDay + _dayTime) * 100.0f : 0.0f;
            skyMaterial.SetShaderParameter("overwritten_time", overwrittenTime);
            skyMaterial.SetShaderParameter("sun_dir_world", _sunDirWorld);
            skyMaterial.SetShaderParameter("moon_dir_world", _moonDirWorld);
        }
    }
}
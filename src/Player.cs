using Godot;
using System;

public partial class Player : CharacterBody3D
{
    // Camera
    public const float FOV_CHANGE = 2.5f;

    public const float BOB_FREQ = 2.0f;
    public const float BOB_AMP = 0.06f;
    public float t_bob = 0f;

    public const float cameraRollIntens = 0.05f;

    const float rollangles = 7.0f;
    const float rollspeed = 300.0f;

    public const float cameraMaxZoom = 60f;
    public float zoomSize = cameraMaxZoom;

    // References
    private CollisionShape3D Collision;
    private CylinderShape3D CollisionSH;
    private SeparationRayShape3D StairCast;
    private Camera3D Camera;
    private Node3D Head;
    private Node3D Hand;
    private RayCast3D AimCast;

    // Movement constants
    private const float MAX_VELOCITY_AIR = 0.6f;
    private const float MAX_VELOCITY_GROUND = 8.0f;
    private const float MAX_VELOCITY_CROUCH = MAX_VELOCITY_GROUND/2;
    private const float MAX_ACCELERATION = 10 * MAX_VELOCITY_GROUND;
    private const float GRAVITY = 15.35f;
    private const float STOP_SPEED = 1.5f;
    private const float STOP_SPEED_CROUCH = .5f;
    private readonly float JUMP_IMPULSE = MathF.Sqrt(2f * GRAVITY * 1f);

    // Movement variables
    public Vector3 _Velocity;
    public Vector3 direction;
    public Vector2 input_direction;

    private bool wish_jump;
    private bool wish_aim;
    private bool wish_crouch;
    private float friction = 4f;

    public bool footstep_can_play;

    // Health variables
    public float Max_hp = 100;
    public float Health = 100;

    // Guns
    public int WeaponIndex = 0;
    private Node3D GunInstance;

    // Signals
    [Signal]
    public delegate void gunSwitchEventHandler(int weaponIDX);


    // Methods

    public override void _Ready()
    {
        // Get the Nodes
        Collision = GetNode<CollisionShape3D>("CollisionSH");
        CollisionSH = (CylinderShape3D)GetNode<CollisionShape3D>("CollisionSH").Shape;
        StairCast = GetNode<CollisionShape3D>("StairCast").Shape as SeparationRayShape3D;
        Camera = GetNode<Camera3D>("Head/Camera3D");
        Head = GetNode<Node3D>("Head");
        Hand = GetNode<Node3D>("Head/Camera3D/Hand");
        AimCast = GetNode<RayCast3D>("Head/Camera3D/AimCast");

        Input.MouseMode = Input.MouseModeEnum.Captured;
        _Velocity = new();

        SwitchWeapons();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            InputEventMouseMotion mouseMotion = @event as InputEventMouseMotion;
            Handle_camera_rotation(mouseMotion);
        }
        else if (@event is InputEventMouseButton)
        {
            InputEventMouseButton mouseButton = @event as InputEventMouseButton;
            if (!wish_aim) DoGunScroll(mouseButton);
            else DoZoomScroll(mouseButton);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Process_Input();
        Process_Movement((float)delta);
        GunInstance.Call("Shoot", AimCast); // Do gun
    }

    public override void _Process(double delta)
    {
        Process_CameraFx((float)delta);
    }

    private void Handle_camera_rotation(InputEventMouseMotion @event)
    {
        RotateY(Mathf.DegToRad(-@event.Relative.X * Settings.SENSITIVITY));
        Head.RotateX(Mathf.DegToRad(-@event.Relative.Y * Settings.SENSITIVITY));

        Vector3 newRot = Head.Rotation;
        newRot.X = Mathf.Clamp(newRot.X, Mathf.DegToRad(-60f), Mathf.DegToRad(90f));
        Head.Rotation = newRot;
    }

    private void Process_Input(/*float _delta*/)
    {
        // Update wishes
        wish_crouch = Input.IsActionPressed("crouch");
        wish_jump = Settings.autoBhop ? Input.IsActionPressed("jump") : Input.IsActionJustPressed("jump");
        wish_aim = Input.IsActionPressed("aim");

        input_direction = Input.GetVector("left", "right", "forward", "backward");

        // Set direction
        direction = new();
        // Better for joystick (camera to be implemented)
        direction += Transform.Basis.Z * input_direction.Y;
        direction += Transform.Basis.X * input_direction.X;
    }

    private void Process_Movement(float delta)
    {
        _Velocity = Velocity; // Movement fix :)

        Vector3 wish_dir = direction.Normalized();

        if (Health <= 0) GetTree().ReloadCurrentScene(); //! Temp

        if (IsOnFloor())
        {
            if (wish_jump)
            {
                _Velocity.Y = JUMP_IMPULSE;
                // Update velocity
                _Velocity = Update_Velocity_Air(wish_dir, delta);
                wish_jump = false;
            }
            else
            {
                _Velocity = wish_crouch ? Update_Velocity_Crouch(wish_dir, delta) : Update_Velocity_Ground(wish_dir, delta);
            }
        }
        else
        {
            _Velocity = Update_Velocity_Air(wish_dir, delta);
            _Velocity.Y -= GRAVITY * delta;
        }

        if (IsOnCeiling()) _Velocity.Y = 0;

        if (wish_crouch)
            StairCast.Length = Mathf.MoveToward(StairCast.Length, 0f, delta * 2.0f);
        else
            StairCast.Length = Mathf.MoveToward(StairCast.Length, 1f, delta * 2.0f);

        Velocity = new(_Velocity.X, _Velocity.Y, _Velocity.Z);

        MoveAndSlide();
    }

    private Vector3 Update_Velocity_Ground(Vector3 wish_dir, float delta)
    {
        float speed = _Velocity.Length();

        if (speed != 0)
        {
            float control = Math.Max(STOP_SPEED, speed);
            float drop = control * friction * delta;

            _Velocity *= Math.Max(speed - drop, 0) / speed;
        }

        return Accelerate(wish_dir, MAX_VELOCITY_GROUND, delta);
    }

    private Vector3 Update_Velocity_Crouch(Vector3 wish_dir, float delta)
    {
        float speed = _Velocity.Length();

        if (speed != 0)
        {
            float control = Math.Max(STOP_SPEED_CROUCH, speed);
            float drop = control * friction/5f * delta;

            _Velocity *= Math.Max(speed - drop, 0) / speed;
        }

        return Accelerate(wish_dir, MAX_VELOCITY_CROUCH, delta);
    }

    private Vector3 Update_Velocity_Air(Vector3 wish_dir, float delta)
    {
        return Accelerate(wish_dir, MAX_VELOCITY_AIR, delta);
    }

    private Vector3 Accelerate(Vector3 wish_dir, float max_velocity, float delta)
    {
        float current_speed = _Velocity.Dot(wish_dir);
        float add_speed = Mathf.Clamp(max_velocity - current_speed, 0, MAX_ACCELERATION * delta);
        return _Velocity + add_speed * wish_dir;
    }

    private void DoGunScroll(InputEventMouseButton mouseButton)
    {
        if (mouseButton.IsPressed())
        {
            if (mouseButton.IsPressed())
            {
                if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                    WeaponIndex += 1;
                if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                    WeaponIndex -= 1;
                SwitchWeapons();
            }
        }
    }

    private void DoZoomScroll(InputEventMouseButton mouseButton)
    {
        // if (!Input.IsActionPressed("Aim")) return;
        if (mouseButton.IsPressed())
        {
            if (mouseButton.IsPressed())
            {
                if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                    zoomSize -= Mathf.Floor(5 / cameraMaxZoom * 100);
                if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                    zoomSize += Mathf.Floor(5 / cameraMaxZoom * 100);

                zoomSize = Mathf.Clamp(zoomSize, 5f, cameraMaxZoom);
            }
        }
    }

    private void SwitchWeapons()
    {
        WeaponIndex %= Hand.GetChildCount(); // Loop value (better for readabillity)

        // Loop for weapon
        for (int i = 0; i < Hand.GetChildCount(); i++)
        {
            Hand.GetChild<Node3D>(i).Hide();
            Hand.GetChild<Node3D>(i).SetProcess(false);
        }

        GunInstance = Hand.GetChild<Node3D>(WeaponIndex);
        GunInstance.Show();

        _ = EmitSignal(SignalName.gunSwitch, WeaponIndex);
    }

    private void Process_CameraFx(float delta)
    {
        Vector3 nRot = new(Camera.Rotation.X, Camera.Rotation.Y, Camera.Rotation.Z);


        float roll = Settings.doCameraRoll ? Calc_Roll(_Velocity, rollangles, rollspeed) : 0;
        nRot.Z = roll;

        Camera.Rotation = nRot;

        Do_HeadBob(delta);

        float aim = wish_aim ? zoomSize : 0f;

        float vel_clamp = Mathf.Clamp(Velocity.Length(), 0, FOV_CHANGE);
        float speed = Settings.Fov + (FOV_CHANGE * vel_clamp);

        float target_fov = speed - aim;
        Camera.Fov = Mathf.Lerp(Camera.Fov, target_fov, delta * 8.0f);
    }

    private void Do_HeadBob(float delta)
    {
        t_bob += delta * Velocity.Length() * (IsOnFloor() ? 1f : 0f) * (!wish_crouch ? 1f : 0f);
        Transform3D transform = Camera.Transform;
        transform.Origin = Calc_HeadBob(t_bob);
        Camera.Transform = transform;
    }

    private static Vector3 Calc_HeadBob(float time)
    {
        Vector3 pos = Vector3.Zero;

        pos.Y = Mathf.Sin(time * BOB_FREQ) * BOB_AMP;
        // pos.X = Mathf.Cos(time * BOB_FREQ/2) * BOB_AMP; // x bob

        return pos;
    }

    private float Calc_Roll(Vector3 velocity, float angle, float speed)
    {
        float s;
        float side;

        side = velocity.Dot(-GlobalTransform.Basis.X);
        s = Math.Sign(side);
        side = Math.Abs(side);

        if (side < speed)
            side = side * angle / speed;
        else
            side = angle;

        return side * s;
    }
}

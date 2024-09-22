open System.Numerics

module Vec2 =
    let create x y = Vector2(x,y)

    let zero  = Vector2.Zero
    let one   = Vector2.One
    let unitX = Vector2.UnitX
    let unitY = Vector2.UnitY

    let inline abs a                   = Vector2.Abs(a)
    let inline add a b                 = Vector2.Add(a,b)
    let inline clamp min max a         = Vector2.Clamp(a,min,max)
    let inline distance p1 p2          = Vector2.Distance(p1,p2)
    let inline distanceSquared p1 p2   = Vector2.DistanceSquared(p1,p2)
    let inline divide  a (b:Vector2)   = Vector2.Divide(a,b)
    let inline dividef a (x:float32)   = Vector2.Divide(a,x)
    let inline dot a b                 = Vector2.Dot(a,b)
    let inline lerp a b t              = Vector2.Lerp(a,b,t)
    let inline max  a b                = Vector2.Max(a,b)
    let inline min  a b                = Vector2.Min(a,b)
    let inline multiply  (a:Vector2) (b:Vector2) = Vector2.Multiply(a,b)
    let inline multiplyf (x:float32) a = Vector2.Multiply(a,x)
    let inline negate a                = Vector2.Negate(a)
    let inline normalize a             = Vector2.Normalize(a)
    let inline reflect normal a        = Vector2.Reflect(a,normal)
    let inline squareRoot a            = Vector2.SquareRoot(a)
    let inline subtract a b            = Vector2.Subtract(a,b)
    let inline xy (v:Vector2)          = v.X, v.Y
    let inline transformM32       (m:Matrix3x2)  a      = Vector2.Transform(a,m)
    let inline transformM44       (m:Matrix4x4)  a      = Vector2.Transform(a,m)
    let inline transformQ         (q:Quaternion) a      = Vector2.Transform(a,q)
    let inline transformNormalM32 (m:Matrix3x2)  normal = Vector2.TransformNormal(normal,m)
    let inline transformNormalM44 (m:Matrix4x4)  normal = Vector2.TransformNormal(normal,m)
    let inline length             (a:Vector2)           = a.Length()
    let inline lengthSquared      (a:Vector2)           = a.LengthSquared()

    let fromPolar rad length =
        create ((cos rad) * length) ((sin rad) * length)

    // rotation matrix:
    // cos(a)  -sin(a)
    // sin(a)   cos(a)
    /// Rotates a Vector by the origin (0,0). Rotation direction depends on how you
    /// view the world. In a game where +X means right and +Y means down because (0,0)
    /// is the TopLeft corner of the screen. Then rotating by +90° rotates something
    /// anti-clockswise.
    let rotate (rad:float32) (v:Vector2) =
        let x = (v.X *  (cos rad)) + (v.Y * (sin rad))
        let y = (v.X * -(sin rad)) + (v.Y * (cos rad))
        create x y

    let deg2rad = System.MathF.PI / 180f
    let rad2deg = 180f / System.MathF.PI

    let rotateDeg (deg:float32) (v:Vector2) =
        rotate (deg * deg2rad) v

    let smoothStep (v1:Vector2) (v2:Vector2) (t:float32) : Vector2 =
        let inline lerp start stop t =
            (start * 1f - t) + (stop * t)

        let inline smoothstep start stop (t:float32) =
            let v1 = t * t
            let v2 = 1.0f - ((1.0f - t) * (1.0f - t))
            lerp start stop (lerp v1 v2 t)

        create
            (smoothstep v1.X v2.X t)
            (smoothstep v1.Y v2.Y t)
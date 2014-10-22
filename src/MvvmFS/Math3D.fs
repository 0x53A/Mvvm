namespace Mvvm.Math3D

[<Measure>]
type radians
[<Measure>]
type degree

/// Vector on a 2D plane
type Vector2(x,y) =
    member self.X = x
    member self.Y = y

/// Vector in 3D space
type Vector3(x,y,z) =

    member self.X = x
    member self.Y = y
    member self.Z = z

    static member (+) (a:Vector3,b:Vector3) =
        Vector3(a.X+b.X, a.Y+b.Y,a.Z+b.Z)

    static member (*) (a:float,b:Vector3) =
        Vector3(a*b.X, a*b.Y,a*b.Z)

    static member (*) (b:Vector3,a:float) =
        Vector3(a*b.X, a*b.Y,a*b.Z)

    static member (/) (a:float,b:Vector3) =
        b * (1.0/a)

    static member (/) (b:Vector3,a:float) =
        b * (1.0/a)

    static member (~-) (v:Vector3) =
        Vector3(-v.X,-v.Y,-v.Z)

    member self.SquaredLength =
        x * x + y * y + z * z

    member self.Length =
        sqrt self.SquaredLength

    member self.IsNormalized =
        self.SquaredLength = 1.0

    member self.Normalize =
        let squaredLength = self.SquaredLength
        if squaredLength <> 1.0 then
            let length = sqrt squaredLength
            let l = length
            Vector3(x/l, y/l, z/l)
        else
            self

    static member Cross (a:Vector3,b:Vector3) =
        let a' = a.Normalize
        let b' = b.Normalize
        let x = a'.Y * b'.Z - a'.Z * b'.Y;
        let y = a'.Z * b'.X - a'.X * b'.Z;
        let z = a'.X * b'.Y - a'.Y * b'.X;
        let l = x*x+y*y+z*z
        Vector3(x/l,y/l,z/l)   
         
    static member (^^) (a:Vector3,b:Vector3) =
        Vector3.Cross(a,b)

    static member Dot(a:Vector3,b:Vector3) =    
        let a' = a.Normalize
        let b' = b.Normalize
        (a'.X * b'.X) + (a'.Y * b'.Y) + (a'.Z * b'.Z)

    static member (.^.) (a:Vector3,b:Vector3) =    
        Vector3.Dot(a,b)


/// Point on a 2D plane
type Point2(x,y) =
    member self.X = x
    member self.Y = y

/// Point in 3D space
type Point3(x,y,z) =
    member self.X = x
    member self.Y = y
    member self.Z = z

    member self.AsVector =
        Vector3(x,y,z)

    static member (+) (a:Point3,b:Vector3) =
        Point3(a.X+b.X, a.Y+b.Y,a.Z+b.Z)


type Vector3 with
    member self.AsPoint =
        Point3(self.X,self.Y,self.Z)

/// A plane in 3D space
type Plane(center:Point3, xAxis:Vector3, yAxis:Vector3) =
    member self.Center = center
    member self.XAxis = xAxis
    member self.YAxis = yAxis
    member self.Map(p:Point2) =
        let x = p.X * xAxis
        let y = p.Y * yAxis
        center + x + y
        

/// Internal helper functions
module math3dinternal =
    let rad2degree (r:float<radians>) =
        r / System.Math.PI * 180.0<degree/radians>

    let degree2rad  (d:float<degree>) =
        d / 180.0<degree/radians> * System.Math.PI

    let areFloatsEqual a b =
        let c = max a b
        let sigma = c / 1000000.0
        let diff = abs (a - b)
        diff < sigma

open math3dinternal

/// Quaternion, representing a Rotation
type Quaternion private (w,x,y,z) =

    member self.W = w
    member self.X = x
    member self.Y = y
    member self.Z = z

    member self.Angle : float<degree> =
        let a_2 = acos w
        let rad = 2.0<radians> * a_2
        rad2degree rad

    member self.Axis =
        let a_2 = acos w
        let _1_sin_a_2 = 1.0 / sin(a_2)
        Vector3(x * _1_sin_a_2, y * _1_sin_a_2, z * _1_sin_a_2)

    static member Identity =
        Quaternion(1.0, 0.0, 0.0, 0.0)

    static member FromAngleAxis(axis:Vector3,angle:float<degree>) =
        let axis' = axis.Normalize        
        let a_2 = (degree2rad angle) / 2.0<radians>
        let w = cos a_2
        let sin_a_2 = sin a_2
        let x = axis'.X * sin_a_2
        let y = axis'.Y * sin_a_2
        let z = axis'.Z * sin_a_2
        Quaternion(w,x,y,z)

    static member FromWXYZ(w,x,y,z) =
        let squaredLength = w * w + x * x + y * y + z * z
        if squaredLength <> 1.0 then
            let length = sqrt squaredLength
            let l = length
            Quaternion(w/l, x/l, y/l, z/l)
        else
            Quaternion(w, x, y, z)

    static member (*) (left:Quaternion,right:Quaternion) =
        let left_w = left.W
        let left_x = left.X
        let left_y = left.Y
        let left_z = left.Z

        let right_w = right.W
        let right_x = right.X
        let right_y = right.Y
        let right_z = right.Z

        let w = (left_w * right_w) - (left_x * right_x) - (left_y * right_y) - (left_z * right_z)

        let x = (left_w * right_x) + (left_x * right_w) + (left_y * right_z) - (left_z * right_y)
        let y = (left_w * right_y) + (left_y * right_w) + (left_z * right_x) - (left_x * right_z)
        let z = (left_w * right_z) + (left_x * right_y) - (left_y * right_x) + (left_z * right_w)

        let length = sqrt(w*w+x*x+y*y+z*z)
        let l = length
        new Quaternion(w/l, x/l, y/l, z/l);

    static member FromVectors (origin:Vector3, destination:Vector3) =
        let o = origin.Normalize
        let d = destination.Normalize
        if o = d then
            Quaternion.Identity
        else
            let axis = o ^^ d
            if axis.Length = 0.0 then
                Quaternion.Identity
            else
                let dot = o .^. d
                let angle = (acos dot) * 1.0<radians>
                let angleDeg = rad2degree angle
                Quaternion.FromAngleAxis(axis,angleDeg)            

    member self.Conjugate =
        Quaternion(w, -x, -y, -z)

    member self.RotateVector (v:Vector3) =
        if v.SquaredLength = 0.0 then
            v
        else
            let qv = Quaternion.FromWXYZ(0.0,v.X,v.Y,v.Z)
            let qn = qv.Conjugate
            let qrot = self * qv * qn
            let v2 = Vector3(qrot.X, qrot.Y, qrot.Z)
            v2 / v2.Length * v.Length
            
    static member (<<|>>) (q:Quaternion,v:Vector3)=
        q.RotateVector(v)
    static member (<<|>>) (v:Vector3,q:Quaternion)=
        q.RotateVector(v)


type LocalCoordinateSystem(translation:Vector3, rotation:Quaternion) =
    let center = Point3(0.0, 0.0, 0.0) + translation
    let xAxis = Vector3(1.0, 0.0, 0.0) <<|>> rotation
    let yAxis = Vector3(0.0, 1.0, 0.0) <<|>> rotation
    let zAxis = Vector3(0.0, 0.0, 1.0) <<|>> rotation
    member self.Translation = translation
    member self.Rotation = rotation
    member self.Center = center
    member self.XAxis = xAxis
    member self.YAxis = yAxis
    member self.ZAxis = zAxis

    member self.MapFromGlobalToLocal(p:Point3) =
        let p' = p + translation
        let p'' = (p'.AsVector <<|>> rotation).AsPoint
        p''

    member self.MapFromLocalToGlobal(p:Point3) =
        let p' = (p.AsVector <<|>> rotation.Conjugate).AsPoint
        let p'' = p' + -translation
        p''
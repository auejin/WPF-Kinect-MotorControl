# WPF-Kinect-MotorControl

Stepper Motor Control with Kinect

It rotates 1D-motor to the exact location of clicked location on depth point image received from Kinect.

You can replace [AngleFromMousePosition()](https://github.com/auejin/WPF-Kinect-MotorControl/blob/82624efa6ecbd2de3f1ee0c087d9ecb513807842/MotorControl-WPF/MainWindow.xaml.cs#L198) as below if you want to get full 2D angle.

```C#
private double[] AngleFromMousePosition(Point position, short depth)
        {
            ...
            return new double[alpha,beta];
}
```

Test Video : [Watch it on Youtube](https://www.youtube.com/watch?v=52IMvLKpYQI)

# CS584 HCI Project @ KAIST

[Full Slideshow](https://docs.google.com/presentation/d/e/2PACX-1vTQjwZHR5yNopmhU3DrwJx8Mj1WJjMoVJQWYtGE6p0W4QhRWvgP8JC8IC5NJSxShIBv_UFA0Np1TkFa/pub?start=false&loop=false&delayms=60000)

[Experiment Video](https://www.youtube.com/watch?v=U3Ipgunlluw)

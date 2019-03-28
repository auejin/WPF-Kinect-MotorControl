# WPF-Kinect-MotorControl

Stepper Motor Control with Kinect

It rotates 1D-motor to the exact location of clicked location on depth point image received from Kinect.

You can replace [AngleFromMousePosition()](https://github.com/auejin/WPF-Kinect-MotorControl/blob/master/MotorControl-WPF/MainWindow.xaml.cs#L198) as below if you want to receive full 2D angle.

```C#
private double[] AngleFromMousePosition(Point position, short depth)
        {
            ...
            return new double[alpha,beta];
}
```

Test Video : [Watch it on Youtube](https://www.youtube.com/watch?v=52IMvLKpYQI)


# ProjectedAssistant; Virtual Assistant with Projector-based Display 

This project is made for *Human Computer Interaction (CS584), KAIST, 2018*.

Majority of virtual assistants have been used voice commands for user interaction. However, virtual assistants with GUI were degrated the locational independency of sound interaction by its fixed visual displays, which makes user not easy to receive informations in a far distance from the virtual assistnaces. Therefore, we suggest a graphic display of virtual assistant with adaptable screen projection. Our graphic display showed 26% more shorter task time comparing to fixed laptop display. Our display also showed 2.7 times lower physical demands and 1.45 times lower mental demand from NASA TLX questionnaire.

You can watch the [Full Slideshow](https://docs.google.com/presentation/d/e/2PACX-1vTQjwZHR5yNopmhU3DrwJx8Mj1WJjMoVJQWYtGE6p0W4QhRWvgP8JC8IC5NJSxShIBv_UFA0Np1TkFa/pub?start=false&loop=false&delayms=60000) or [Experiment Video](https://www.youtube.com/watch?v=U3Ipgunlluw) of this project.

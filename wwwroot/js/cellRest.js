const root = document.documentElement;

const primaryColor1 = getComputedStyle(root).getPropertyValue(
    "--primary-color-1"
);
const primaryColor2 = getComputedStyle(root).getPropertyValue(
    "--primary-color-2"
);
const startGradientAnimationValues = `${primaryColor2}; ${primaryColor1}; ${primaryColor2}`;
const endGradientAnimationValues = `${primaryColor1}; ${primaryColor2}; ${primaryColor1}`;
const animationDuration = getComputedStyle(root).getPropertyValue(
    "--animation-duration"
);
const animationDurationGradient = `${animationDuration.split("s")[0] * 2}s`;

const startGradient = document.getElementById("startGradient");
const endGradient = document.getElementById("endGradient");
const startGradientAnimation = document.getElementById(
    "startGradientAnimation"
);
const endGradientAnimation = document.getElementById("endGradientAnimation");

startGradient.setAttribute("stop-color", primaryColor2);
endGradient.setAttribute("stop-color", primaryColor1);
startGradientAnimation.setAttribute("values", startGradientAnimationValues);
endGradientAnimation.setAttribute("values", endGradientAnimationValues);
startGradientAnimation.setAttribute("dur", animationDurationGradient);
endGradientAnimation.setAttribute("dur", animationDurationGradient);

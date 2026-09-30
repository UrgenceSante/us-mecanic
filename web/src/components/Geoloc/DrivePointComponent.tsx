import { Marker } from "react-map-gl/maplibre";
import type { DrivePoint, StopPoint } from "./Geoloc.model";
import ArrowDropUpIcon from "@mui/icons-material/ArrowDropUp";

interface DrivePointComponentProps {
  drivepoint: DrivePoint;
  onClick: (p: StopPoint) => void;
}
export default function DrivePointComponent(props: DrivePointComponentProps) {
  const getSpeedGradientColor = (speed: number, min = 0, max = 20): string => {
    const clamp = (val: number, min: number, max: number) =>
      Math.max(min, Math.min(val, max));

    // Normalise la vitesse entre 0 et 1
    const ratio = clamp((speed - min) / (max - min), 0, 1);

    // Interpolation entre rouge (255, 0, 0) et vert (0, 200, 0)
    const r = Math.round(255 * (1 - ratio));
    const g = Math.round(200 * ratio);
    const b = 0;

    return `rgb(${r},${g},${b})`;
  };

  const handleOnClick = () => {
    const p: StopPoint = {
      Latitude: props.drivepoint.Latitude,
      DurationInSecond: 0,
      EndDateTime: props.drivepoint.LocalTime,
      HasEngineOff: false,
      Longitude: props.drivepoint.Longitude,
      StartDatetime: props.drivepoint.LocalTime,
    };
    props.onClick(p);
  };

  const color = getSpeedGradientColor(props.drivepoint.Speed);
  return (
    <Marker
      latitude={props.drivepoint.Latitude}
      longitude={props.drivepoint.Longitude}
      onClick={handleOnClick}
    >
      <ArrowDropUpIcon
        // color={color}

        fontSize="small"
        sx={{
          rotate: `${props.drivepoint.Heading}deg`,
          color: color,
        }}
      />
      {/* <div
        style={{
          width: "5px",
          height: "5px",
          backgroundColor: "blue",
          borderRadius: "50%",
        }}
      /> */}
    </Marker>
  );
}

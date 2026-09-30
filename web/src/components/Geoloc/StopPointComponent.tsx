import { Marker } from "react-map-gl/maplibre";
import type { StopPoint } from "./Geoloc.model";
import { Box, Chip } from "@mui/material";

interface StopPointComponentProps {
  stopPoint: StopPoint;
  onClick: (p: StopPoint) => void;
  isDepart?: boolean;
  isArrive?: boolean;
}
export default function StopPointComponent(props: StopPointComponentProps) {
  const timeString = new Date(props.stopPoint.StartDatetime).toLocaleTimeString(
    "fr-FR",
    { minute: "2-digit", hour: "2-digit" }
  );

  const durationString =
    (props.stopPoint.DurationInSecond / 60).toFixed(0) + "min";

  const color = props.isDepart
    ? "success"
    : props.isArrive
    ? "warning"
    : "default";

  return (
    <Marker
      latitude={props.stopPoint.Latitude}
      longitude={props.stopPoint.Longitude}
      onClick={() => props.onClick(props.stopPoint)}
    >
      <Box position={"relative"}>
        <Chip
          size="small"
          color={color}
          label={`${timeString} ${durationString}`}
        />
      </Box>
    </Marker>
  );
}

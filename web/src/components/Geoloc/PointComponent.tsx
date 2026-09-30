import { Marker, Popup } from "react-map-gl/maplibre";
import type { Point } from "./Geoloc.model";
import { Box } from "@mui/material";
import { useState } from "react";

interface PointComponentProps {
  point: Point;
  color?: string;
  label?: string;
  popupLabel?: string;
}
export default function PointComponent(props: PointComponentProps) {
  const [isHover, setIsHover] = useState(false);

  return (
    <Marker latitude={props.point.Latitude} longitude={props.point.Longitude}>
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          width: "12px",
          height: "12px",
          color: "white",
          padding: "10px",
          backgroundColor: props.color,
          borderRadius: "50%",
          border: "1px solid white",
          boxShadow: "0 0 3px black",
          fontWeight: "600",
        }}
        onMouseEnter={() => setIsHover(true)}
        onMouseLeave={() => setIsHover(false)}
      >
        {props.label || ""}
      </Box>
      {isHover && (
        <Popup
          latitude={props.point.Latitude}
          longitude={props.point.Longitude}
          closeButton={false}
          closeOnClick={false}
          offset={12}
        >
          <Box sx={{ fontSize: 14 }}>
            {props.popupLabel || props.point.Label || "N/A"}
          </Box>
        </Popup>
      )}
    </Marker>
  );
}

import { Box, Slider, Typography } from "@mui/material";
import type { StopPoint } from "./Geoloc.model";
import { time } from "../Utils/DateTime.service";
import HourglassBottomIcon from "@mui/icons-material/HourglassBottom";

interface GeolocTimelineProps {
  min: number;
  max: number;
  value: number[];
  onChange: (value: number[]) => void;
  pointsOfInterest: StopPoint[];
}
export default function GeolocTimeline(props: GeolocTimelineProps) {
  const valueLabelFormat = (value: number) => {
    return new Date(value).toLocaleTimeString();
  };

  const marks = props.pointsOfInterest.map((p) => ({
    value: time(p.StartDatetime),
    label: (
      <Box display={"flex"} flexDirection={"column"} alignItems={"center"}>
        <HourglassBottomIcon color="secondary" />
        <Typography>{`${(p.DurationInSecond / 60).toFixed(0)}min`}</Typography>
      </Box>
    ),
    // label: `${(p.DurationInSecond / 60).toFixed(0)}min`,
  }));

  return (
    <Box position={"relative"} width={"100%"} height={"100px"} padding={"24px"}>
      <Slider
        valueLabelDisplay="on"
        value={props.value}
        onChange={(_e, value) => props.onChange(value)}
        min={props.min}
        max={props.max}
        marks={marks}
        valueLabelFormat={valueLabelFormat}
        step={1000}
      >
        <Box sx={{ width: "25px", height: "10px", backgroundColor: "coral" }} />
      </Slider>
      {/* <Box>
        {props.pointsOfInterest.map((p) => (
          <Tooltip key={p.StartDatetime} title={p.StartDatetime}>
            <Box
              sx={{
                position: "absolute",
                left: `${
                  ((time(p.StartDatetime) - props.min) /
                    (props.max - props.min)) *
                  100
                }%`,
                width: 8,

                height: 20,
                borderRadius: 2,
                backgroundColor: orange[500],
              }}
            />
          </Tooltip>
        ))}
      </Box> */}
    </Box>
  );
}

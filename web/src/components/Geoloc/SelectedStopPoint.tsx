import {
  Button,
  Card,
  CardContent,
  CardHeader,
  IconButton,
  Typography,
} from "@mui/material";
import type { StopPoint } from "./Geoloc.model";
import { grey } from "@mui/material/colors";
import VisibilityIcon from "@mui/icons-material/Visibility";

export default function SelectedStopPoint(props: {
  point?: StopPoint;
  title: string;
  onClick: (p: StopPoint) => void;
  onDelete: () => void;
  distance?: number;
}) {
  const { point, title, onClick, distance } = props;

  const cardContent = point ? (
    <>
      <CardContent>
        <Typography>x: {point.Latitude}</Typography>
        <Typography>y: {point.Longitude}</Typography>

        <Button
          onClick={() => props.onDelete()}
          sx={{ padding: 0, marginTop: 3 }}
          color="error"
        >
          Supprimer
        </Button>
      </CardContent>
    </>
  ) : (
    <>
      <CardContent>Selectionnez un point</CardContent>
    </>
  );

  return (
    <Card sx={{ backgroundColor: grey[100] }}>
      <CardHeader
        title={<Typography>{title}</Typography>}
        subheader={
          distance ? <Typography>{distance?.toFixed(0)} m</Typography> : null
        }
        action={
          point && (
            <IconButton onClick={() => onClick(point)}>
              <VisibilityIcon />
            </IconButton>
          )
        }
      />
      {cardContent}
    </Card>
  );
}

import { Card, CardHeader, IconButton, Typography } from "@mui/material";
import type { Point } from "./Geoloc.model";
import VisibilityIcon from "@mui/icons-material/Visibility";
import { grey } from "@mui/material/colors";

//TODO: Changer le nom pour quelque chose de plus cohérent
export default function MainPoint(props: {
  color: string;
  title?: string;
  point: Point;
  onClick: (p: Point) => void;
}) {
  const { title, point, color, onClick } = props;
  return (
    <Card sx={{ backgroundColor: color || grey[500] }}>
      <CardHeader
        title={<Typography fontSize={14}>{title}</Typography>}
        action={
          point && (
            <IconButton onClick={() => onClick(point)}>
              <VisibilityIcon />
            </IconButton>
          )
        }
      />
    </Card>
  );
}

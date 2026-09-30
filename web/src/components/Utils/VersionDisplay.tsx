import { Box, Typography } from "@mui/material";
import packagejson from "../../../package.json";
import { appConfig } from "../../config/appConfig";

const version = packagejson.version || "x.x.x";

export default function VersionDisplay() {
  return (
    <Box sx={{ textAlign: "center" }}>
      <Typography variant="caption" color="text.secondary">
        Version {version} – {appConfig.envName}
      </Typography>
    </Box>
  );
}

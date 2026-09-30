import { Box, Typography } from "@mui/material";
import logoLoader from "../../Assets/Images/logo-loader.gif";

export default function LogoLoader(props: { LoadingText?: string }) {
  return (
    <Box sx={{ display: "flex", justifyContent: "center", flexDirection: "column", alignItems: "center" }}>
      <img width={250} src={logoLoader} alt="logo qui tourne" />
      {props.LoadingText && <Typography>{props.LoadingText}</Typography>}
    </Box>
  );
}

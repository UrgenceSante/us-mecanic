import { Outlet } from "react-router";
import {
  AppBar,
  Avatar,
  Box,
  Container,
  Toolbar,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import logo from "../Assets/Images/logo-us.png";
import UserAvatar from "../components/User/UserAvatar";
import DesktopNav from "./layout/DesktopNav";
import MobileNav from "./layout/MobileNav";

export default function AppLayout() {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up("md"));

  return (
    <Box display={"flex"} flexDirection={"column"} gap={2} height={"100%"}>
      <AppBar position="relative">
        <Toolbar sx={{ gap: 1 }}>
          {!isDesktop && <MobileNav />}
          <Avatar src={logo} />
          {isDesktop && <DesktopNav />}

          <Box flexGrow={1} />

          <UserAvatar />
        </Toolbar>
      </AppBar>
      <Container sx={{ flex: 1 }}>
        <Outlet />
      </Container>
    </Box>
  );
}

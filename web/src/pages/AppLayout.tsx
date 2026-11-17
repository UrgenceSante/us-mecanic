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
import SkipLink from "../shared/components/SkipLink";
import DesktopNav from "./layout/DesktopNav";
import MobileNav from "./layout/MobileNav";

export default function AppLayout() {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up("sm"));

  return (
    <Box display={"flex"} flexDirection={"column"} gap={2} height={"100%"}>
      <SkipLink />
      <AppBar position="relative">
        <Toolbar sx={{ gap: 1 }}>
          {!isDesktop && <MobileNav />}
          <Avatar src={logo} alt="" />
          {isDesktop && <DesktopNav />}

          <Box flexGrow={1} />

          <UserAvatar />
        </Toolbar>
      </AppBar>
      <Container component="main" id="main" tabIndex={-1} sx={{ flex: 1, outline: "none" }}>
        <Outlet />
      </Container>
    </Box>
  );
}

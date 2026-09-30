import { Link, Outlet } from "react-router";
import { AppBar, Avatar, Box, Button, Container, Toolbar } from "@mui/material";
import logo from "../Assets/Images/logo-us.png";
import UserAvatar from "../components/User/UserAvatar";

export default function AppLayout() {
  return (
    <Box display={"flex"} flexDirection={"column"} gap={2} height={"100%"}>
      <AppBar position="relative">
        <Toolbar>
          <Avatar src={logo} />
          <Button color="inherit" component={Link} to="/">
            Logs
          </Button>
          <Button
            color="inherit"
            component={Link}
            to="/geoloc/FS414ZX/bae74f53-8d18-47c8-bd0b-0b9ead081aad"
          >
            Geoloc
          </Button>
          <Button
            color="inherit"
            component={Link}
            to="/notification-subscriptions"
          >
            Subscriptions
          </Button>

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

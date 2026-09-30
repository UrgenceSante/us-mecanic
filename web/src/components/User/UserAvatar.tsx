import {
  Avatar,
  Box,
  Button,
  Divider,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
} from "@mui/material";
import { useKeycloak } from "@react-keycloak/web";
import { useState } from "react";
import { UserInitials } from "../Utils/Keycloak/UserInitials";
import { deepOrange } from "@mui/material/colors";
import LogoutIcon from "@mui/icons-material/Logout";

export default function UserAvatar() {
  const { keycloak, initialized } = useKeycloak();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  if (initialized && keycloak.authenticated)
    return (
      <>
        <Box display="flex" gap={1} alignItems="center">
          <IconButton
            onClick={handleClick}
            size="small"
            sx={{ ml: 2 }}
            // aria-controls={open ? "account-menu" : undefined}
            // aria-haspopup="true"
            // aria-expanded={open ? "true" : undefined}
          >
            <Avatar
              sx={{
                width: 24,
                height: 24,
                padding: 2.5,
                bgcolor: deepOrange[500],
              }}
            >
              {keycloak.tokenParsed ? UserInitials(keycloak.tokenParsed) : "?"}
            </Avatar>
          </IconButton>
        </Box>
        <Menu
          anchorEl={anchorEl}
          // id="account-menu"
          open={open}
          onClose={handleClose}
          onClick={handleClose}
          slotProps={{
            paper: {
              elevation: 0,
              sx: {
                overflow: "visible",
                filter: "drop-shadow(0px 2px 8px rgba(0,0,0,0.32))",
                mt: 1.5,
                "& .MuiAvatar-root": {
                  width: 32,
                  height: 32,
                  ml: -0.5,
                  mr: 1,
                },
                "&::before": {
                  content: '""',
                  display: "block",
                  position: "absolute",
                  top: 0,
                  right: 14,
                  width: 10,
                  height: 10,
                  bgcolor: "background.paper",
                  transform: "translateY(-50%) rotate(45deg)",
                  zIndex: 0,
                },
              },
            },
          }}
          transformOrigin={{ horizontal: "right", vertical: "top" }}
          anchorOrigin={{ horizontal: "right", vertical: "bottom" }}
        >
          <MenuItem>
            <ListItemText>
              {keycloak.tokenParsed?.name || "Erreur token"}
            </ListItemText>
          </MenuItem>

          <Divider />

          <MenuItem onClick={() => keycloak.logout()}>
            <ListItemIcon>
              <LogoutIcon fontSize="small" />
            </ListItemIcon>
            Logout
          </MenuItem>
        </Menu>
      </>
    );

  return (
    <Button variant="text" sx={{color: "white"}} onClick={() => keycloak.login()}>
      Login
    </Button>
  );
}

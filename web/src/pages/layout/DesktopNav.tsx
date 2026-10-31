import { useState } from "react";
import { Link, useLocation } from "react-router";
import { Box, Button, Menu, MenuItem } from "@mui/material";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import {
  adminNavItems,
  isNavItemActive,
  mainNavItems,
} from "./navigationConfig";

export default function DesktopNav() {
  const { pathname } = useLocation();
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const adminActive = adminNavItems.some((i) => isNavItemActive(i, pathname));

  return (
    <Box component="nav" aria-label="Navigation principale" display="flex">
      {mainNavItems.map((item) => {
        const active = isNavItemActive(item, pathname);
        return (
          <Button
            key={item.to}
            color="inherit"
            component={Link}
            to={item.to}
            aria-current={active ? "page" : undefined}
            sx={{
              fontWeight: active ? 700 : 400,
              borderBottom: active ? 2 : 0,
              borderRadius: 0,
            }}
          >
            {item.label}
          </Button>
        );
      })}
      <Button
        color="inherit"
        aria-haspopup="menu"
        aria-expanded={anchorEl ? "true" : undefined}
        endIcon={<ArrowDropDownIcon />}
        onClick={(e) => setAnchorEl(e.currentTarget)}
        sx={{ fontWeight: adminActive ? 700 : 400 }}
      >
        Administration
      </Button>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={() => setAnchorEl(null)}
      >
        {adminNavItems.map((item) => (
          <MenuItem
            key={item.to}
            component={Link}
            to={item.to}
            selected={isNavItemActive(item, pathname)}
            onClick={() => setAnchorEl(null)}
          >
            {item.label}
          </MenuItem>
        ))}
      </Menu>
    </Box>
  );
}

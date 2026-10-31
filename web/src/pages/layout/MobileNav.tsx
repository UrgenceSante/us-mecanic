import { useState } from "react";
import { Link, useLocation } from "react-router";
import {
  Box,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  ListSubheader,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import {
  adminNavItems,
  isNavItemActive,
  mainNavItems,
  type NavItem,
} from "./navigationConfig";

interface NavGroupProps {
  title: string;
  items: NavItem[];
  pathname: string;
  onNavigate: () => void;
}

function NavGroup({ title, items, pathname, onNavigate }: NavGroupProps) {
  return (
    <List
      aria-label={title}
      subheader={<ListSubheader component="div">{title}</ListSubheader>}
    >
      {items.map((item) => {
        const active = isNavItemActive(item, pathname);
        return (
          <ListItemButton
            key={item.to}
            component={Link}
            to={item.to}
            selected={active}
            aria-current={active ? "page" : undefined}
            onClick={onNavigate}
            sx={{
              minHeight: 48,
              borderLeft: 4,
              borderColor: active ? "primary.main" : "transparent",
              "& .MuiListItemText-primary": { fontWeight: active ? 700 : 400 },
            }}
          >
            <ListItemText primary={item.label} />
          </ListItemButton>
        );
      })}
    </List>
  );
}

export default function MobileNav() {
  const { pathname } = useLocation();
  const [open, setOpen] = useState(false);
  const close = () => setOpen(false);

  return (
    <>
      <IconButton
        color="inherit"
        aria-label="Ouvrir le menu"
        edge="start"
        onClick={() => setOpen(true)}
        sx={{ width: 48, height: 48 }}
      >
        <MenuIcon />
      </IconButton>
      <Drawer anchor="left" open={open} onClose={close}>
        <Box
          component="nav"
          aria-label="Navigation"
          sx={{ width: 280, maxWidth: "85vw" }}
        >
          <NavGroup
            title="Suivi"
            items={mainNavItems}
            pathname={pathname}
            onNavigate={close}
          />
          <NavGroup
            title="Administration"
            items={adminNavItems}
            pathname={pathname}
            onNavigate={close}
          />
        </Box>
      </Drawer>
    </>
  );
}

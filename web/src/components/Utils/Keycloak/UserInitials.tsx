import type { KeycloakTokenParsed } from "keycloak-js";

interface TokenParsed extends KeycloakTokenParsed{
  given_name?: string;
  family_name?: string;
  name?: string;
  preferred_username?: string;
}

export function UserInitials(tokenParsed: TokenParsed): string {
  const { given_name, family_name, name, preferred_username } = tokenParsed;

  if (given_name && family_name) {
    return `${given_name[0]}${family_name[0]}`.toUpperCase();
  }

  if (name) {
    const parts = name.trim().split(" ");
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    } else {
      return parts[0][0].toUpperCase();
    }
  }

  if (preferred_username) {
    return preferred_username.slice(0, 2).toUpperCase();
  }

  return "?";
}

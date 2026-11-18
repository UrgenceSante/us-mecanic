import { useState } from "react";
import { OWNER_MISSING } from "../components/VehicleCompanyFilter";
import type { VehicleFilter, VehicleStatus } from "../types/vehicle";

/** État des filtres du parc et critères correspondants pour l'API. */
export default function useVehicleListFilters() {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState<VehicleStatus | "">("");
  const [type, setType] = useState("");
  const [owner, setOwner] = useState("");
  const [currentUser, setCurrentUser] = useState("");

  const filter: VehicleFilter = {
    search,
    status: status || undefined,
    type: type || undefined,
    ownerCompanyId: owner && owner !== OWNER_MISSING ? owner : undefined,
    ownerMissing: owner === OWNER_MISSING,
    currentUserCompanyId: currentUser || undefined,
  };
  const reset = () => {
    setSearch("");
    setStatus("");
    setType("");
    setOwner("");
    setCurrentUser("");
  };

  return {
    values: { search, status, type, owner, currentUser },
    setters: { setSearch, setStatus, setType, setOwner, setCurrentUser },
    filter,
    filtered: search !== "" || status !== "" || type !== "" || owner !== "" || currentUser !== "",
    reset,
  };
}

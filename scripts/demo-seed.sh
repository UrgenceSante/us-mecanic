#!/usr/bin/env bash
# Charge des données de démonstration FICTIVES dans us-mecanic, via l'API publique uniquement.
# Usage : scripts/demo-seed.sh [--dry-run] [API_URL]      (API_URL peut aussi venir de l'environnement)
# Prérequis : bash, curl, jq, date GNU. Aucune écriture directe en base.
set -euo pipefail
shopt -s inherit_errexit # les erreurs dans $(...) arrêtent aussi le script
export TZ=Europe/Paris # même « aujourd'hui » que l'API

DRY_RUN=0
API_URL="${API_URL:-}"
for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=1 ;;
    -h | --help) sed -n '2,4p' "$0"; exit 0 ;;
    *) API_URL="$arg" ;;
  esac
done
API_URL="${API_URL:-http://localhost:5255}"
API_URL="${API_URL%/}"

command -v jq >/dev/null || { echo "jq est requis." >&2; exit 1; }
command -v curl >/dev/null || { echo "curl est requis." >&2; exit 1; }
date -d "today" +%F >/dev/null 2>&1 || { echo "date GNU requis (sous macOS : coreutils, gdate)." >&2; exit 1; }

step() { printf '\n== %s\n' "$*" >&2; }
info() { printf '   %s\n' "$*" >&2; }
die() { printf 'ERREUR : %s\n' "$*" >&2; exit 1; }
dt() { date -d "$*" +%F; }          # date relative à aujourd'hui, ex. dt "-12 months +15 days"
off() { printf '%+d days' "$1"; }   # décalage en jours signé
dq() { printf '"%s"' "$(dt "$1")"; } # date relative entre guillemets JSON

DRY_COUNTER=0
# api METHODE CHEMIN [CORPS_JSON] : écrit le corps de la réponse sur stdout ; arrêt avec le ProblemDetails si erreur.
api() {
  local method="$1" path="$2" body="${3:-}"
  if ((DRY_RUN)); then
    printf '   %s %s %s\n' "$method" "$path" "$body" >&2
    DRY_COUNTER=$((DRY_COUNTER + 1))
    if [[ "$method" == GET ]]; then echo '[]'; else printf '{"id":"00000000-0000-0000-0000-%012d"}\n' "$DRY_COUNTER"; fi
    return 0
  fi
  local out code
  local args=(-sS -X "$method" -w $'\n%{http_code}' "$API_URL$path")
  if [[ -n "$body" ]]; then args+=(-H 'Content-Type: application/json' -d "$body"); fi
  out=$(curl "${args[@]}") || die "appel impossible : $method $path"
  code="${out##*$'\n'}"
  out="${out%$'\n'*}"
  if [[ "$code" != 2* ]]; then
    printf 'ERREUR : %s %s -> HTTP %s\n%s\n' "$method" "$path" "$code" "$(jq . <<<"$out" 2>/dev/null || echo "$out")" >&2
    if [[ -n "$body" ]]; then printf 'Corps envoyé : %s\n' "$body" >&2; fi
    exit 1
  fi
  printf '%s' "$out"
}
post() { api POST "$@" | jq -r '.id'; }

# ensure CHEMIN NOM [CORPS_SUPPLEMENTAIRE_JSON] : id de l'élément portant ce nom (créé s'il n'existe pas).
ensure() {
  local path="$1" name="$2" extra="${3:-}" id
  if [[ -z "$extra" ]]; then extra='{}'; fi
  id=$(api GET "$path?includeInactive=true" | jq -r --arg n "$name" '[.[] | select(.name == $n)][0].id // empty')
  if [[ -z "$id" ]]; then
    id=$(post "$path" "$(jq -nc --arg n "$name" --argjson e "$extra" '{name: $n} + $e')")
    info "créé : $name"
  fi
  printf '%s' "$id"
}

# --- Garde-fous ---------------------------------------------------------------------------------
step "Vérifications préalables ($API_URL)"
if ((DRY_RUN)); then
  info "mode --dry-run : aucune requête n'est envoyée (garde-fous ignorés)"
else
  code=$(curl -sS -o /dev/null -w '%{http_code}' "$API_URL/api/vehicles") || die "API injoignable sur $API_URL"
  case "$code" in
    200) info "API en authentification désactivée : OK" ;;
    401 | 403) die "API authentifiée : refus d'exécuter le script de démo" ;;
    *) die "réponse inattendue de l'API (HTTP $code) sur GET /api/vehicles" ;;
  esac
  if api GET "/api/vehicles?search=DM-" | jq -e 'any(.[]; .licensePlate | startswith("DM-"))' >/dev/null; then
    echo "La démo est déjà chargée (véhicules DM-xxx-AA présents) : rien à faire." >&2
    exit 0
  fi
fi

# --- Référentiels -------------------------------------------------------------------------------
step "Types de véhicule"
declare -A VTYPE
for t in "Ambulance" "VSL" "Corbillard" "Véhicule funéraire" "Transport PSL"; do VTYPE[$t]=$(ensure /api/vehicle-types "$t"); done

step "Types d'échéance (types de base déjà présents ; ajout de ceux propres à la désinfection et au PSL)"
declare -A DTYPE
for t in "Contrôle technique" "Assurance" "Entretien constructeur" "Agrément ARS" "Matériel médical" "Oxygène" "Extincteur"; do
  DTYPE[$t]=$(ensure /api/deadline-types "$t")
done
DTYPE["Désinfection"]=$(ensure /api/deadline-types "Désinfection" '{"defaultIntervalMonths":3}')
DTYPE["Chaîne du froid"]=$(ensure /api/deadline-types "Chaîne du froid" '{"defaultIntervalMonths":12}')

step "Garages"
declare -A GARAGE
for g in "Garage Démo Centre" "Carrosserie Démo Ouest" "Pneus Démo Express" "Atelier Froid Démo" "Désinfection Démo Services" "Équipement Médical Démo"; do
  GARAGE[$g]=$(ensure /api/garages "$g")
done

step "Référentiels des problèmes"
declare -A NATURE ELEMENT ACTOR ATYPE CONSTRAINT
for n in "Panne" "Usure" "Dégradation" "Défaut d'équipement"; do NATURE[$n]=$(ensure /api/problem-natures "$n"); done
for n in "Brancard" "Gyrophare" "Climatisation" "Porte arrière" "Groupe froid" "Plateau de chargement funéraire"; do
  ELEMENT[$n]=$(ensure /api/problem-elements "$n")
done
for n in "Atelier interne" "Garage partenaire" "Constructeur" "Responsable de parc"; do ACTOR[$n]=$(ensure /api/problem-actors "$n"); done
for n in "Réparation" "Remplacement de pièce" "Diagnostic" "Contrôle après réparation"; do ATYPE[$n]=$(ensure /api/problem-action-types "$n"); done
for n in "Aucune" "Attente de pièce" "Disponibilité du véhicule" "Budget à valider"; do CONSTRAINT[$n]=$(ensure /api/problem-constraints "$n"); done

step "Catégories de dépense"
declare -A CATEGORY
for c in "Nettoyage-désinfection" "Équipement médical" "Pneumatiques" "Péage et stationnement" "Accessoires et uniformes"; do
  CATEGORY[$c]=$(ensure /api/expense-categories "$c")
done

step "Sociétés fictives"
declare -A COMPANY
for c in "Ambulances Démo" "Pompes Funèbres Démo" "Transport PSL Démo"; do COMPANY[$c]=$(ensure /api/companies "$c"); done
# Société propriétaire selon le type ; le véhicule 11 (sorti du parc) reste sans propriétaire (« Propriétaire à renseigner »).
owner_of() {
  case "$1" in
    Ambulance | VSL) printf '%s' "${COMPANY[Ambulances Démo]}" ;;
    Corbillard | "Véhicule funéraire") printf '%s' "${COMPANY[Pompes Funèbres Démo]}" ;;
    "Transport PSL") printf '%s' "${COMPANY[Transport PSL Démo]}" ;;
    *) printf '' ;;
  esac
}

# --- Véhicules ----------------------------------------------------------------------------------
# n | type | marque | modèle | âge (ans) | km il y a 12 mois | km par mois
# | décalage d'échéance du CT (jours, <0 = dépassée) | idem assurance | km restants avant l'entretien (<0 = dépassé)
VEHICLES='1|Ambulance|Renault|Master|4|52000|2400|-20|120|800
2|Ambulance|Mercedes-Benz|Sprinter|6|88000|2800|12|200|-500
3|Ambulance|Volkswagen|Crafter|3|21000|2600|200|45|15000
4|VSL|Peugeot|508 SW|5|60000|1900|90|300|6000
5|VSL|Skoda|Superb Combi|4|47000|1700|5|18|2500
6|Corbillard|Mercedes-Benz|Classe E break|7|70000|900|150|-10|9000
7|Corbillard|Volvo|V90|3|15000|800|300|260|20000
8|Véhicule funéraire|Renault|Trafic|5|58000|1500|-3|75|400
9|Transport PSL|Renault|Kangoo|2|25000|2900|60|150|12000
10|Transport PSL|Peugeot|Partner|4|44000|2300|240|28|3000
11|Ambulance|Ford|Transit|9|130000|0|100|100|10000'

declare -A VID PLATE CUR TYPEOF OFF_CT OFF_INS KM_LEFT INC
step "Véhicules et relevés kilométriques sur 12 mois"
while IFS='|' read -r n type brand model age base inc ct ins left; do
  plate=$(printf 'DM-%03d-AA' "$n")
  PLATE[$n]=$plate
  TYPEOF[$n]=$type
  OFF_CT[$n]=$ct
  OFF_INS[$n]=$ins
  KM_LEFT[$n]=$left
  INC[$n]=$inc
  owner=$(owner_of "$type")
  if ((n == 11)); then owner=''; fi
  VID[$n]=$(post /api/vehicles "$(jq -nc --arg o "$owner" --arg p "$plate" --arg b "$brand" --arg m "$model" --arg d "$(dt "-$age years -3 months")" \
    --arg vin "$(printf 'VFDM%013d' "$n")" --arg i "$(printf 'DEMO-%02d' "$n")" --arg t "${VTYPE[$type]}" \
    '{licensePlate: $p, details: ({brand: $b, model: $m, firstRegistrationDate: $d, vin: $vin, internalNumber: $i, typeId: $t} + (if $o == "" then {} else {ownerCompanyId: $o} end))}')")
  info "$plate $type $brand $model"
  CUR[$n]=$base
  for month in 12 11 10 9 8 7 6 5 4 3 2 1 0; do # un relevé par mois, du plus ancien à aujourd'hui
    if [[ "$inc" == 0 && "$month" -lt 12 ]]; then continue; fi # véhicule sorti du parc : un seul relevé
    km=$((base + (12 - month) * inc))
    api POST "/api/vehicles/${VID[$n]}/mileage-readings" \
      "$(jq -nc --arg d "$(dt "-$month months")" --argjson k "$km" '{date: $d, kilometers: $k}')" >/dev/null
    CUR[$n]=$km
  done
done <<<"$VEHICLES"

# --- Échéances ----------------------------------------------------------------------------------
# add_periodic N TYPE MOIS KM DATE_DERNIERE [KM_DERNIER]  (MOIS/KM : nombre ou null)
add_periodic() {
  local n="$1" type="$2" months="$3" km="$4" last="$5" lastkm="${6:-null}"
  api POST "/api/vehicles/${VID[$n]}/deadlines" "$(jq -nc --arg t "${DTYPE[$type]}" --argjson m "$months" --argjson k "$km" \
    --arg d "$last" --argjson lk "$lastkm" '{deadlineTypeId: $t, intervalMonths: $m, intervalKm: $k, lastDoneDate: $d, lastDoneKm: $lk}')" >/dev/null
}
step "Échéances (proches, dépassées et à jour, calculées par rapport à aujourd'hui)"
for n in $(seq 1 10); do
  add_periodic "$n" "Contrôle technique" 12 null "$(dt "-12 months $(off "${OFF_CT[$n]}")")"
  add_periodic "$n" "Assurance" 12 null "$(dt "-12 months $(off "${OFF_INS[$n]}")")"
  # Dernière révision il y a 5 mois, au km du relevé de ce jour-là ; intervalle arrondi au millier pour
  # qu'il reste environ KM_LEFT km avant la prochaine.
  km5=$((CUR[$n] - 5 * INC[$n]))
  add_periodic "$n" "Entretien constructeur" 12 $(((CUR[$n] - km5 + KM_LEFT[$n] + 500) / 1000 * 1000)) "$(dt "-5 months")" "$km5"
  add_periodic "$n" "Extincteur" 12 null "$(dt "-12 months $(off $((n * 37 % 90 - 30)))")"
  case "${TYPEOF[$n]}" in
    Ambulance | VSL)
      if [[ "${TYPEOF[$n]}" == Ambulance ]]; then # oxygène et matériel médical : ambulances seulement
        add_periodic "$n" "Oxygène" 6 null "$(dt "-6 months $(off $((n * 11 % 40 - 10)))")"
        add_periodic "$n" "Matériel médical" 6 null "$(dt "-6 months $(off $((n * 17 % 60 - 5)))")"
      fi
      add_periodic "$n" "Désinfection" 3 null "$(dt "-3 months $(off $((n * 7 % 30 - 8)))")"
      # Agrément ARS : échéance ponctuelle (date fixée directement)
      api POST "/api/vehicles/${VID[$n]}/deadlines" "$(jq -nc --arg t "${DTYPE[Agrément ARS]}" --arg d "$(dt "$(off $((n * 53 % 400 - 15)))")" \
        '{deadlineTypeId: $t, dueDate: $d}')" >/dev/null ;;
    "Transport PSL")
      add_periodic "$n" "Chaîne du froid" 12 null "$(dt "-12 months $(off $((n * 13 % 70 - 10)))")"
      add_periodic "$n" "Désinfection" 3 null "$(dt "-3 months $(off $((n * 5 % 25 - 5)))")" ;;
    *) ;;
  esac
  info "${PLATE[$n]} : échéances ajoutées"
done

# --- Interventions ------------------------------------------------------------------------------
# intervention N JOURS_AVANT GARAGE MONTANT TRAVAUX : écrit la réponse JSON
intervention() {
  api POST "/api/vehicles/${VID[$1]}/interventions" "$(jq -nc --arg d "$(dt "-$2 days")" --arg g "${GARAGE[$3]}" --argjson a "$4" --arg w "$5" \
    '{date: $d, garageId: $g, amount: $a, work: $w}')"
}
step "Interventions du carnet d'entretien"
while IFS='|' read -r n days garage amount work; do intervention "$n" "$days" "$garage" "$amount" "$work" >/dev/null; done <<'EOT'
1|200|Garage Démo Centre|642.50|Vidange, filtres et plaquettes de frein avant
1|45|Pneus Démo Express|480.00|Remplacement de deux pneus avant
2|150|Garage Démo Centre|1180.90|Remplacement de l'embrayage
3|90|Équipement Médical Démo|215.00|Révision du matériel médical embarqué
4|120|Garage Démo Centre|389.00|Vidange et courroie d'accessoires
5|30|Pneus Démo Express|612.40|Train de pneus hiver
6|240|Garage Démo Centre|520.00|Entretien constructeur
7|60|Carrosserie Démo Ouest|950.00|Reprise de peinture de l'aile arrière gauche
8|75|Carrosserie Démo Ouest|310.00|Remplacement du rétroviseur droit
9|100|Atelier Froid Démo|760.00|Contrôle et recharge du groupe froid
10|20|Désinfection Démo Services|95.00|Désinfection complète de la cellule
EOT

# --- Problèmes ----------------------------------------------------------------------------------
# report N JOURS_AVANT DESCRIPTION AUTEUR : id du problème
report() {
  post /api/problems "$(jq -nc --arg p "${PLATE[$1]}" --arg d "$(dt "-$2 days")" --arg s "$3" --arg a "$4" \
    '{licensePlate: $p, date: $d, description: $s, author: $a}')"
}
# analyze PROBLEME NATURE ELEMENT TEXTE IMMOBILISE(true|false)
analyze() {
  api PUT "/api/problems/$1/analysis" "$(jq -nc --arg n "${NATURE[$2]}" --arg e "${ELEMENT[$3]}" --arg t "$4" --argjson i "$5" \
    '{natureId: $n, elementId: $e, analysis: $t, immobilized: $i}')" >/dev/null
}
# action PROBLEME ACTEUR TYPE CONTRAINTE ECHEANCE(null|"date") COMMENTAIRE [INTERVENTION_JSON] : id de l'action
action() {
  post "/api/problems/$1/actions" "$(jq -nc --arg a "${ACTOR[$2]}" --arg t "${ATYPE[$3]}" --arg c "${CONSTRAINT[$4]}" --argjson d "$5" \
    --arg m "$6" --argjson i "${7:-null}" '{actorId: $a, typeId: $t, constraintId: $c, dueDate: $d, comment: $m, interventionId: $i}')"
}

step "Problèmes à tous les statuts"
info "Signalés (non analysés)"
report 1 3 "Gyrophare avant droit intermittent" "Chauffeur Démo A" >/dev/null
report 4 1 "La climatisation de l'habitacle ne refroidit plus" "Chauffeur Démo B" >/dev/null

info "Analysé, non immobilisant, action à venir"
p=$(report 3 12 "La porte arrière se verrouille difficilement" "Chauffeur Démo C")
analyze "$p" "Usure" "Porte arrière" "Serrure et charnière usées, jeu constaté à la fermeture." false
action "$p" "Garage partenaire" "Remplacement de pièce" "Attente de pièce" "$(dq '+10 days')" "Pièces commandées" >/dev/null

info "Analysé, immobilisant, actions en retard (ambulance immobilisée)"
p=$(report 2 18 "Le brancard ne se relève plus : vérin bloqué" "Chauffeur Démo D")
analyze "$p" "Panne" "Brancard" "Vérin de relevage hors service : véhicule indisponible pour les transports." true
action "$p" "Constructeur" "Diagnostic" "Aucune" "$(dq '-9 days')" "Diagnostic attendu du fournisseur" >/dev/null
action "$p" "Garage partenaire" "Remplacement de pièce" "Budget à valider" "$(dq '-2 days')" "Devis du vérin à valider" >/dev/null

info "Analysé, immobilisant, action proche (véhicule PSL immobilisé)"
p=$(report 9 5 "Le groupe froid ne descend plus sous 8 °C" "Chauffeur Démo E")
analyze "$p" "Panne" "Groupe froid" "Compresseur défaillant : transport de produits sanguins impossible." true
action "$p" "Garage partenaire" "Réparation" "Disponibilité du véhicule" "$(dq '+3 days')" "Intervention de l'Atelier Froid Démo" >/dev/null

info "Résolus (toutes les actions closes, l'une liée à une intervention)"
p=$(report 8 40 "Le rail du plateau de chargement funéraire est faussé" "Chauffeur Démo F")
analyze "$p" "Dégradation" "Plateau de chargement funéraire" "Rail déformé lors d'un chargement." false
inter=$(intervention 8 28 "Carrosserie Démo Ouest" 430.00 "Remplacement du rail du plateau de chargement" | jq -r '.id')
a=$(action "$p" "Garage partenaire" "Réparation" "Aucune" "$(dq '-30 days')" "Rail remplacé" "$(jq -nc --arg i "$inter" '$i')")
api POST "/api/problems/$p/actions/$a/close" >/dev/null

p=$(report 6 60 "La climatisation de l'habitacle du corbillard ne démarre plus" "Chauffeur Démo G")
analyze "$p" "Panne" "Climatisation" "Recharge de gaz nécessaire, compresseur en bon état." false
a=$(action "$p" "Atelier interne" "Réparation" "Aucune" "$(dq '-50 days')" "Recharge effectuée")
api POST "/api/problems/$p/actions/$a/close" >/dev/null

# --- Dépenses -----------------------------------------------------------------------------------
# N | catégorie | jours (<0 passé, >0 prévue) | montant | libellé
step "Dépenses (dont prévues)"
while IFS='|' read -r n category days amount label; do
  api POST "/api/vehicles/${VID[$n]}/expenses" "$(jq -nc --arg c "${CATEGORY[$category]}" --arg d "$(dt "$(off "$days")")" --argjson a "$amount" --arg l "$label" \
    '{date: $d, categoryId: $c, amount: $a, label: $l}')" >/dev/null
done <<'EOT'
1|Nettoyage-désinfection|-60|85.00|Désinfection après transport infectieux
1|Pneumatiques|-45|120.00|Équilibrage et géométrie
2|Équipement médical|-120|340.00|Remplacement du matelas de brancard
3|Accessoires et uniformes|-30|59.90|Housses de siège et tapis
4|Péage et stationnement|-14|27.40|Péages mission longue distance
5|Pneumatiques|-20|240.00|Valves et équilibrage train hiver
6|Nettoyage-désinfection|-10|64.00|Lavage et finition intérieure
8|Accessoires et uniformes|-90|210.00|Rideaux et tapis de plateau
9|Équipement médical|-25|180.00|Étalonnage de la sonde de température
10|Nettoyage-désinfection|-7|45.00|Désinfection de la cellule
3|Équipement médical|+20|450.00|Prévue : renouvellement du défibrillateur
9|Nettoyage-désinfection|+35|120.00|Prévue : désinfection complète de la cellule PSL
EOT

# --- Emprunts -----------------------------------------------------------------------------------
# N | prêteur | montant | mensualité | début (mois avant) | durée (mois)
step "Emprunts"
while IFS='|' read -r n lender amount monthly start duration; do
  api POST "/api/vehicles/${VID[$n]}/loans" "$(jq -nc --arg l "$lender" --argjson a "$amount" --argjson m "$monthly" \
    --arg s "$(dt "-$start months")" --argjson d "$duration" \
    '{lender: $l, borrowedAmount: $a, monthlyPayment: $m, startDate: $s, durationMonths: $d}')" >/dev/null
done <<'EOT'
3|Banque Démo|62000.00|1150.00|10|60
7|Crédit-bail Démo|58000.00|980.00|8|60
9|Banque Démo|24000.00|450.00|6|60
10|Financement Démo Auto|21000.00|395.00|30|60
EOT

# --- Locations ----------------------------------------------------------------------------------
# N | société locataire | début (jours, <0 passé) | fin (jours, ou « - » : sans date de fin, à terminer à la main)
# Terminée (DM-001), en cours sans fin pour montrer « Terminer » (DM-004), prévue (DM-007).
step "Locations entre sociétés (terminée, en cours, prévue)"
while IFS='|' read -r n tenant start end; do
  if [[ "$end" == "-" ]]; then end_json=null; else end_json=$(dq "$(off "$end")"); fi
  api POST "/api/vehicles/${VID[$n]}/rentals" "$(jq -nc --arg t "${COMPANY[$tenant]}" --arg s "$(dt "$(off "$start")")" --argjson e "$end_json" \
    '{tenantCompanyId: $t, startDate: $s, endDate: $e}')" >/dev/null
  info "${PLATE[$n]} loué à $tenant"
done <<'EOT'
1|Transport PSL Démo|-60|-30
4|Pompes Funèbres Démo|-10|-
7|Ambulances Démo|15|30
EOT

# --- Statut final -------------------------------------------------------------------------------
step "Véhicule sorti du parc"
api PUT "/api/vehicles/${VID[11]}/status" '{"status":"Retired"}' >/dev/null
info "${PLATE[11]} : sorti du parc"

step "Terminé"
if ((DRY_RUN)); then info "dry-run : $DRY_COUNTER requêtes d'écriture simulées"; else info "Démo chargée : 11 véhicules DM-001-AA à DM-011-AA"; fi

import { useEffect, useState } from 'react'
import { Alert, CircularProgress, Stack, Typography } from '@mui/material'

import { getSubscriptions } from './subscription.service'
import type { Subscription } from './Subscription'
import SubscriptionList from './SubscriptionList'

export default function SubscriptionContainer() {

    const [subscriptions, setSubscriptions] = useState<Subscription[]>([])
    const [loading, setLoading] = useState(true)
    const [failed, setFailed] = useState(false)

    useEffect(() => {
        getSubscriptions()
            .then(res => setSubscriptions(res))
            .catch(() => setFailed(true))
            .finally(() => setLoading(false))
    }, [])

    return (
        <Stack gap={2} marginY={2}>
            <Typography variant="h5">Abonnements</Typography>
            {loading && <CircularProgress aria-label="Chargement" sx={{ alignSelf: 'center' }} />}
            {failed && <Alert severity="error">Impossible de charger les abonnements.</Alert>}
            {!loading && !failed && subscriptions.length === 0 && (
                <Typography color="text.secondary">Aucun abonnement.</Typography>
            )}
            {!loading && !failed && subscriptions.length > 0 && <SubscriptionList subscriptions={subscriptions} />}
        </Stack>
    )
}

import { useEffect, useState } from 'react'

import { getSubscriptions } from './subscription.service'
import type { Subscription } from './Subscription'
import SubscriptionList from './SubscriptionList'

export default function SubscriptionContainer() {

    const [subscriptions, setSubscriptions] = useState<Subscription[]>([])
    const [err, setErr] = useState("")

    useEffect(() => {
        getSubscriptions().then(res => setSubscriptions(res)).catch(err => setErr(err.message || "Une erreur s'est produite"))
    }, [])

    if (err) return <div>{err} </div>
    return (
        <SubscriptionList subscriptions={subscriptions} />
    )
}

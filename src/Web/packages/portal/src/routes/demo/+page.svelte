<script lang="ts">
    import { Button } from "@nocturne/ui/ui/button";
    import { DEMO_ENABLED, DEMO_WEB_URL } from "$lib/config";
</script>

<svelte:head>
    <title>Demo - Nocturne</title>
</svelte:head>

{#if DEMO_ENABLED && DEMO_WEB_URL}
    <div class="flex flex-col h-[calc(100vh-4rem)]">
        <div class="px-4 py-3 border-b border-border/60 bg-card/50 flex items-center justify-between">
            <div>
                <h1 class="text-lg font-semibold">Live Demo</h1>
                <p class="text-sm text-muted-foreground">
                    Realistic demo data — resets periodically
                </p>
            </div>
            <Button
                href={DEMO_WEB_URL}
                target="_blank"
                rel="noopener noreferrer"
                variant="outline"
                size="sm"
            >
                Open in new tab
            </Button>
        </div>
        <iframe
            src={DEMO_WEB_URL}
            title="Nocturne Demo"
            class="flex-1 w-full border-0"
            allow="fullscreen"
        ></iframe>
    </div>
{:else}
    <div class="container mx-auto px-4 py-20">
        <div class="max-w-md mx-auto text-center">
            <h1 class="text-2xl font-bold mb-4">Demo Not Available</h1>
            <p class="text-muted-foreground mb-6">
                The demo instance is not currently enabled.
            </p>
            <Button href="/" variant="outline">
                Back to Home
            </Button>
        </div>
    </div>
{/if}

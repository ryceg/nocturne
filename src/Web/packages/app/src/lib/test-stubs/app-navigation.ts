/**
 * Stub for $app/navigation in browser test environment.
 */
export function goto(_url: string, _opts?: any) {
  return Promise.resolve();
}

export function invalidate(_url: string) {
  return Promise.resolve();
}

export function invalidateAll() {
  return Promise.resolve();
}

export function beforeNavigate(_callback: any) {}

export function afterNavigate(_callback: any) {}

export function onNavigate(_callback: any) {}

export function replaceState(_url: string, _state?: any) {}

export function pushState(_url: string, _state?: any) {}

export function preloadData(_url: string) {
  return Promise.resolve();
}

export function preloadCode(..._urls: string[]) {
  return Promise.resolve();
}

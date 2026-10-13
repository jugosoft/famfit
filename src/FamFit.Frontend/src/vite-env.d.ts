/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string;
  readonly VITE_BACKEND_TARGET?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

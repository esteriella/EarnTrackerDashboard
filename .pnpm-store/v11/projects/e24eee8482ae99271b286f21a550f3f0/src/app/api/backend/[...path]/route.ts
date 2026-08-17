import { NextRequest, NextResponse } from "next/server";

export const dynamic = "force-dynamic";

type RouteContext = { params: Promise<{ path: string[] }> };

function apiBaseUrl() {
  return (
    process.env.EARNTRACKER_API_URL ??
    process.env.NEXT_PUBLIC_API_URL ??
    "http://localhost:5048"
  ).replace(/\/$/, "");
}

async function forward(request: NextRequest, context: RouteContext) {
  const { path } = await context.params;
  const target = `${apiBaseUrl()}/${path.map(encodeURIComponent).join("/")}${request.nextUrl.search}`;
  const headers = new Headers();
  const authorization = request.headers.get("authorization");
  const contentType = request.headers.get("content-type");

  if (authorization) headers.set("authorization", authorization);
  if (contentType) headers.set("content-type", contentType);
  headers.set("accept", "application/json");

  try {
    const upstream = await fetch(target, {
      method: request.method,
      headers,
      body: request.method === "GET" || request.method === "HEAD"
        ? undefined
        : await request.arrayBuffer(),
      cache: "no-store",
      redirect: "follow",
    });
    const body = await upstream.arrayBuffer();

    return new NextResponse(body, {
      status: upstream.status,
      headers: {
        "content-type": upstream.headers.get("content-type") ?? "application/json",
      },
    });
  } catch {
    return NextResponse.json(
      { detail: "The earnings service is unavailable. It may be waking up; please try again shortly." },
      { status: 502 },
    );
  }
}

export const GET = forward;
export const POST = forward;
export const PUT = forward;
export const PATCH = forward;
export const DELETE = forward;
